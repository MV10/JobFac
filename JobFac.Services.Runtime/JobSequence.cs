using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Microsoft.Extensions.Configuration;
using Orleans;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading.Tasks;

namespace JobFac.Services.Runtime
{
    public class JobSequence : Grain, IJobSequence
    {
        private string jobInstanceKey = null;
        private JobStatus<StatusSequence> status = null;
        private JobDefinition<DefinitionSequence> jobDefinition = null;

        private readonly DefinitionsRepository definitionRepo;
        private readonly HistoryRepository historyRepo;

        public JobSequence(
            DefinitionsRepository definitions,
            HistoryRepository history)
        {
            definitionRepo = definitions;
            historyRepo = history;
        }

        public override async Task OnActivateAsync()
        {
            jobInstanceKey = this.GetPrimaryKeyString();
            var history = await historyRepo.GetJobHistory(jobInstanceKey);
            if (history != null) status = historyRepo.DeserializeDetails<StatusSequence>(history);
        }

        public async Task Start(JobDefinition<DefinitionSequence> jobDefinition, FactoryStartOptions options)
        {
            if (status != null)
                throw new Exception($"Job has already been started (instance {jobInstanceKey})");

            jobDefinition.ThrowIfInvalid();

            this.jobDefinition = jobDefinition;

            status = new JobStatus<StatusSequence>
            {
                Key = jobInstanceKey,
                StartOptions = options,
                LastUpdated = DateTimeOffset.UtcNow,
            };
            await StartNextStep(); // this will increment the step number and write to history repository
        }

        public Task<JobStatus<StatusSequence>> GetStatus()
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            return Task.FromResult(status);
        }

        public async Task JobStatusChanged(JobStatus<StatusExternalProcess> jobStatus)
        {
            throw new NotImplementedException();
        }

        public async Task Stop()
        {
            throw new NotImplementedException();
        }

        private async Task StartNextStep()
        {
            status.JobTypeProperties.SequenceStep++;
            var step = new StatusSequenceStep { Step = status.JobTypeProperties.SequenceStep };
            status.JobTypeProperties.StepStatus.Add(step.Step, step);



            await historyRepo.InsertStatus(status);

        }
    }
}
