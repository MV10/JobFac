using JobFac.database;
using JobFac.lib.DataModels;
using JobFac.services;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.runtime
{
    [StatelessWorker]
    public class JobFactory : Grain, IJobFactory
    {
        private readonly IClusterClient clusterClient;
        private readonly DefinitionsRepository definitionRepo;

        public JobFactory(
            IClusterClient client,
            DefinitionsRepository defRepo)
        {
            clusterClient = client;
            definitionRepo = defRepo;
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

            if (options.StartupPayloads.ContainsKey(id) && !jobDefinition.PrefixJobInstanceIdArgument)
                throw new Exception($"Job definition {id} does not support startup payloads");

            var jobInstanceKey = Guid.NewGuid().ToString();
            var jobGrain = clusterClient.GetGrain<IJob>(jobInstanceKey);
            await jobGrain.Start(jobDefinition, options);

            return jobInstanceKey;
        }

        public async Task<string> StartSequence(FactoryStartOptions options)
        {
            options.ThrowIfInvalid();

            // TODO actually start a sequence

            var sequenceInstanceKey = Guid.NewGuid().ToString();
            return sequenceInstanceKey;
        }
    }
}
