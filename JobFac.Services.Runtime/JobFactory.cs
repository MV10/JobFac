using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
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

        public async Task<string> StartJob(FactoryStartOptions options, string replacementArguments = null, string startupPayload = null)
        {
            if (replacementArguments != null)
            {
                options.ReplacementArguments.Clear();
                options.ReplacementArguments.Add(options.DefinitionId, replacementArguments);
            }

            if (startupPayload != null)
            {
                options.StartupPayloads.Clear();
                options.StartupPayloads.Add(options.DefinitionId, startupPayload);
            }

            return await StartJob(options);
        }

        public async Task<string> StartJob(FactoryStartOptions options)
        {
            options.ThrowIfInvalid();

            var jobType = await definitionRepo.GetJobType(options.DefinitionId);

            return jobType switch
            {
                JobType.ExternalProcess => await StartJobExternalProcess(options),
                JobType.Sequence => await StartJobSequence(options),
                _ => throw new Exception($"Job definition {options.DefinitionId} is not a type that can be started"),
            };
        }

        public async Task<IReadOnlyList<string>> GetRunningJobInstanceIds(string definitionId)
            => await historyRepo.GetActiveJobInstanceIds(definitionId);

        private async Task<string> StartJobExternalProcess(FactoryStartOptions options)
        {
            var id = options.DefinitionId;

            var jobDefinition = await definitionRepo.GetJobDefinition<DefinitionExternalProcess>(id);
            if (jobDefinition == null)
                throw new Exception("Invalid job definition id");

            if (options.ReplacementArguments.ContainsKey(id) && !jobDefinition.JobTypeProperties.AllowReplacementArguments)
                throw new Exception($"Job definition {id} does not allow replacement arguments");

            if (options.StartupPayloads.ContainsKey(id) && !jobDefinition.JobTypeProperties.IsJobFacAware)
                throw new Exception($"Job definition {id} is not JobFac-aware and doesn't support startup payloads");

            await ProcessAlreadyRunningJobs(jobDefinition);

            var jobInstanceKey = Formatting.NewInstanceKey;
            
            var jobGrain = GrainFactory.GetGrain<IJobExternalProcess>(jobInstanceKey);
            await jobGrain.Start(jobDefinition, options);
            
            return jobInstanceKey;
        }

        private async Task<string> StartJobSequence(FactoryStartOptions options)
        {
            var id = options.DefinitionId;

            var jobDefinition = await definitionRepo.GetJobDefinition<DefinitionSequence>(id);
            if (jobDefinition == null)
                throw new Exception("Invalid job definition id");

            await ProcessAlreadyRunningJobs(jobDefinition);

            var jobInstanceKey = Formatting.NewInstanceKey;
            options.SequenceInstanceId = jobInstanceKey;

            var jobGrain = GrainFactory.GetGrain<IJobSequence>(jobInstanceKey);
            await jobGrain.Start(jobDefinition, options);
            
            return jobInstanceKey;
        }

        private async Task ProcessAlreadyRunningJobs(JobDefinitionBase jobDefinition)
        {
            if (jobDefinition.AlreadyRunningAction != AlreadyRunningAction.StartNormally)
            {
                var instances = await historyRepo.GetActiveJobInstanceIds(jobDefinition.Id);
                if (instances.Count > 0)
                {
                    // TODO job-already-running notifications

                    if (jobDefinition.AlreadyRunningAction == AlreadyRunningAction.DoNotStart)
                        throw new Exception($"Job definition {jobDefinition.Id} is already running and is not configured to start additional instances");

                    if (jobDefinition.AlreadyRunningAction == AlreadyRunningAction.StopOthersBeforeStarting)
                    {
                        foreach (var key in instances)
                        {
                            var otherJob = GrainFactory.GetGrain<IJobExternalProcess>(key);
                            await otherJob.Stop();
                        }
                    }
                }
            }
        }
    }
}
