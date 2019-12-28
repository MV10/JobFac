using JobFac.Library.Constants;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Library.Database
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
            => await QueryOneAsync<JobHistory>(ConstQueries.SelectJobHistory, new { InstanceKey = instanceKey }).ConfigureAwait(false);

        public async Task<SequenceHistory> GetSequenceHistory(string instanceKey)
            => await QueryOneAsync<SequenceHistory>(ConstQueries.SelectSequenceHistory, new { InstanceKey = instanceKey }).ConfigureAwait(false);

        public async Task<IReadOnlyList<JobHistory>> GetJobHistory(string definitionId, DateTimeOffset fromDate)
            => await QueryAsync<JobHistory>(ConstQueries.SelectJobHistoryAfter, new { DefinitionId = definitionId, FromDate = fromDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<SequenceHistory>> GetSequenceHistory(string definitionId, DateTimeOffset fromDate)
            => await QueryAsync<SequenceHistory>(ConstQueries.SelectSequenceHistoryAfter, new { DefinitionId = definitionId, FromDate = fromDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<JobHistory>> GetJobHistory(string definitionId, DateTimeOffset firstDate, DateTimeOffset lastDate)
            => await QueryAsync<JobHistory>(ConstQueries.SelectJobHistoryBetween, new { DefinitionId = definitionId, FirstDate = firstDate.UtcDateTime, LastDate = lastDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<SequenceHistory>> GetSequenceHistory(string definitionId, DateTimeOffset firstDate, DateTimeOffset lastDate)
            => await QueryAsync<SequenceHistory>(ConstQueries.SelectSequenceHistoryBetween, new { DefinitionId = definitionId, FirstDate = firstDate.UtcDateTime, LastDate = lastDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<string>> GetActiveJobInstanceIds(string definitionId)
            => await QueryAsync<string>(ConstQueries.SelectJobHistoryActive, new { DefinitionId = definitionId }).ConfigureAwait(false);

        public async Task<IReadOnlyList<string>> GetActiveSequenceInstanceIds(string definitionId)
            => await QueryAsync<string>(ConstQueries.SelectSequenceHistoryActive, new { DefinitionId = definitionId }).ConfigureAwait(false);

        public async Task InsertStatus(JobStatus status)
            => await ExecAsync(ConstQueries.InsertJobHistory, History(status)).ConfigureAwait(false);

        public async Task InsertStatus(SequenceStatus status)
            => await ExecAsync(ConstQueries.InsertSequenceHistory, History(status)).ConfigureAwait(false);

        public async Task UpdateStatus(JobStatus status)
            => await ExecAsync(ConstQueries.UpdateJobHistory, History(status)).ConfigureAwait(false);

        public async Task UpdateStatus(SequenceStatus status)
            => await ExecAsync(ConstQueries.UpdateSequenceHistory, History(status)).ConfigureAwait(false);

        public async Task InsertCapturedOutput(string instanceKey, string stdOut, string stdErr)
        {
            var payload = new
            {
                InstanceKey = instanceKey,
                StdOut = stdOut.HasContent() ? stdOut : string.Empty,
                StdErr = stdErr.HasContent() ? stdErr : string.Empty
            };
            await ExecAsync(ConstQueries.InsertCapturedOutput, payload).ConfigureAwait(false);
        }

        public async Task PurgeHistory()
            => await ExecAsync(ConstQueries.PurgeHistory, new { Now = DateTimeOffset.UtcNow }).ConfigureAwait(false);

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
                FinalRunStatus = status.HasExited ? status.RunStatus : RunStatus.Unknown,
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
                FinalRunStatus = status.HasExited ? status.RunStatus : RunStatus.Unknown,
                FullDetailsJson = JsonConvert.SerializeObject(status)
            };
    }
}
