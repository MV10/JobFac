using JobFac.lib.Constants;
using JobFac.lib.DataModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.database
{
    public class HistoryRepository : DapperRepositoryBase
    {
        private readonly int historyRetentionDays;

        public HistoryRepository(IConfiguration config)
            : base(config)
        {
            historyRetentionDays = config.GetValue<int>(ConstConfigKeys.HistoryRetentionDays);
        }

        public async Task<JobHistory> GetJobHistory(string instanceKey)
            => await QueryOneAsync<JobHistory>(ConstQueries.SelectJobHistory, new { InstanceKey = instanceKey });

        public async Task<SequenceHistory> GetSequenceHistory(string instanceKey)
            => await QueryOneAsync<SequenceHistory>(ConstQueries.SelectSequenceHistory, new { InstanceKey = instanceKey });

        public async Task<IReadOnlyList<JobHistory>> GetJobHistory(string definitionId, DateTimeOffset fromDate)
            => await QueryAsync<JobHistory>(ConstQueries.SelectJobHistoryAfter, new { DefinitionId = definitionId, FromDate = fromDate.UtcDateTime });

        public async Task<IReadOnlyList<SequenceHistory>> GetSequenceHistory(string definitionId, DateTimeOffset fromDate)
            => await QueryAsync<SequenceHistory>(ConstQueries.SelectSequenceHistoryAfter, new { DefinitionId = definitionId, FromDate = fromDate.UtcDateTime });

        public async Task<IReadOnlyList<JobHistory>> GetJobHistory(string definitionId, DateTimeOffset firstDate, DateTimeOffset lastDate)
            => await QueryAsync<JobHistory>(ConstQueries.SelectJobHistoryBetween, new { DefinitionId = definitionId, FirstDate = firstDate.UtcDateTime, LastDate = lastDate.UtcDateTime });

        public async Task<IReadOnlyList<SequenceHistory>> GetSequenceHistory(string definitionId, DateTimeOffset firstDate, DateTimeOffset lastDate)
            => await QueryAsync<SequenceHistory>(ConstQueries.SelectSequenceHistoryBetween, new { DefinitionId = definitionId, FirstDate = firstDate.UtcDateTime, LastDate = lastDate.UtcDateTime });

        public async Task InsertStatus(JobStatus status)
            => await ExecAsync(ConstQueries.InsertJobHistory, History(status));

        public async Task InsertStatus(SequenceStatus status)
            => await ExecAsync(ConstQueries.InsertSequenceHistory, History(status));

        public async Task UpdateStatus(JobStatus status)
            => await ExecAsync(ConstQueries.UpdateJobHistory, History(status));

        public async Task UpdateStatus(SequenceStatus status)
            => await ExecAsync(ConstQueries.UpdateSequenceHistory, History(status));

        public async Task PurgeHistory()
            => await ExecAsync(ConstQueries.PurgeHistory, new { Now = DateTimeOffset.UtcNow });

        public JobStatus DeserializeDetails(JobHistory history)
            => JsonConvert.DeserializeObject<JobStatus>(history.FullDetailsJson);

        public SequenceStatus DeserializeDetails(SequenceHistory history)
            => JsonConvert.DeserializeObject<SequenceStatus>(history.FullDetailsJson);

        private JobHistory History(JobStatus status)
            => new JobHistory
            {
                InstanceKey = status.Key,
                DefinitionId = status.StartOptions.DefinitionId,
                LastUpdated = status.LastUpdated,
                DeleteAfter = status.LastUpdated.AddDays(historyRetentionDays).Date,
                FinalRunStatus = status.RunStatus,
                ExitCode = status.ExitCode,
                FullDetailsJson = JsonConvert.SerializeObject(status)
            };

        private SequenceHistory History(SequenceStatus status)
            => new SequenceHistory
            {
                InstanceKey = status.Key,
                DefinitionId = status.StartOptions.DefinitionId,
                LastUpdated = status.LastUpdated,
                DeleteAfter = status.LastUpdated.AddDays(historyRetentionDays).Date,
                FinalRunStatus = status.RunStatus,
                FullDetailsJson = JsonConvert.SerializeObject(status)
            };
    }
}
