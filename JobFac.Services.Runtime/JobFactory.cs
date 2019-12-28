using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services.Runtime
{
    [StatelessWorker]
    public class JobFactory : Grain, IJobFactory
    {
        private readonly DefinitionsRepository definitionRepo;
        private readonly HistoryRepository historyRepo;

        public JobFactory(
            DefinitionsRepository definitionRepo,
            HistoryRepository historyRepo)
        {
            this.definitionRepo = definitionRepo;
            this.historyRepo = historyRepo;
        }

        public async Task<string> StartJob(FactoryStartOptions options)
        {
            options.ThrowIfInvalid();

            var id = options.DefinitionId;

            var jobDefinition = await definitionRepo.GetJobDefinition(id);
            if (jobDefinition == null)
                throw new Exception("Invalid job definition id");

            if (options.ReplacementArguments.ContainsKey(id) && !jobDefinition.AllowReplacementArguments)
                throw new Exception($"Job definition {id} does not allow replacement arguments");

            if (options.StartupPayloads.ContainsKey(id) && !jobDefinition.IsJobFacAware)
                throw new Exception($"Job definition {id} is not JobFac-aware and doesn't support startup payloads");

            if (jobDefinition.AlreadyRunningAction != AlreadyRunningAction.StartNormally)
            {
                var instances = await historyRepo.GetActiveJobInstanceIds(id);
                if(instances.Count > 0)
                {
                    // TODO job-already-running notifications

                    if (jobDefinition.AlreadyRunningAction == AlreadyRunningAction.DoNotStart)
                        throw new Exception($"Job definition {id} is already running and is not configured to start additional instances");

                    if(jobDefinition.AlreadyRunningAction == AlreadyRunningAction.StopOthersBeforeStarting)
                    {
                        foreach(var key in instances)
                        {
                            var otherJob = GrainFactory.GetGrain<IJob>(key);
                            await otherJob.Stop();
                        }
                    }
                }
            }

            var jobInstanceKey = Formatting.NewInstanceKey;
            var jobGrain = GrainFactory.GetGrain<IJob>(jobInstanceKey);
            await jobGrain.Start(jobDefinition, options);

            return jobInstanceKey;
        }

        public async Task<string> StartJob(FactoryStartOptions options, string replacementArguments = null, string startupPayload = null)
        {
            if(replacementArguments != null)
            {
                options.ReplacementArguments.Clear();
                options.ReplacementArguments.Add(options.DefinitionId, replacementArguments);
            }

            if(startupPayload != null)
            {
                options.StartupPayloads.Clear();
                options.StartupPayloads.Add(options.DefinitionId, startupPayload);
            }

            return await StartJob(options);
        }

        public async Task<string> StartSequence(FactoryStartOptions options)
        {
            options.ThrowIfInvalid();

            // TODO actually start a sequence

            var sequenceInstanceKey = Formatting.NewInstanceKey;
            return sequenceInstanceKey;
        }

        public async Task<IReadOnlyList<string>> GetRunningJobInstanceIds(string definitionId)
        {
            var jobDefinition = await definitionRepo.GetJobDefinition(definitionId);
            if (jobDefinition == null)
                throw new Exception("Invalid job definition id");

            return await historyRepo.GetActiveJobInstanceIds(definitionId);
        }

        public async Task<IReadOnlyList<string>> GetRunningSequenceInstanceIds(string definitionId)
        {
            var jobDefinition = await definitionRepo.GetSequenceDefinition(definitionId);
            if (jobDefinition == null)
                throw new Exception("Invalid sequence definition id");

            return await historyRepo.GetActiveSequenceInstanceIds(definitionId);
        }
    }
}
