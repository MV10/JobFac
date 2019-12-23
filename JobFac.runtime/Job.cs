using JobFac.database;
using JobFac.lib.DataModels;
using JobFac.services;
using Orleans;
using System;
using System.Threading.Tasks;

namespace JobFac.runtime
{
    public class Job : Grain, IJob
    {
        private JobStatus status;

        private readonly HistoryRepository historyRepo;

        public Job(HistoryRepository history)
        {
            historyRepo = history;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var history = await historyRepo.GetJobHistory(key);
            if(history == null)
            {
                status = new JobStatus
                {
                    Key = key,
                    LastUpdated = DateTimeOffset.UtcNow,
                    RunStatus = RunStatus.Unknown,
                    HasStarted = false,
                    HasCompleted = false,
                    HasFailed = false
                };
                await historyRepo.InsertStatus(status);
            }
            else
            {
                status = historyRepo.DeserializeDetails(history);
            }
        }

        private async Task WriteStatus()
        {
            // TODO call history repo to insert/update status
        }

        public Task Start(JobDefinition jobDefinition, FactoryStartOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<JobDefinition> GetDefinition()
        {
            throw new NotImplementedException();
        }

        public Task<JobStatus> GetStatus()
        {
            throw new NotImplementedException();
        }

        public Task UpdateExitMessage(RunStatus runStatus, string exitMessage)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRunStatus(RunStatus runStatus)
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
