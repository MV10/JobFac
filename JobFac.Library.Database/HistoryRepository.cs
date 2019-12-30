using JobFac.Library;
using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
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

        public async Task<JobHistoryTable> GetJobHistory(string instanceKey)
            => await QueryOneRowAsync<JobHistoryTable>(ConstQueries.SelectJobHistory, new { InstanceKey = instanceKey }).ConfigureAwait(false);

        public async Task<IReadOnlyList<JobHistoryTable>> GetJobHistory(string definitionId, DateTimeOffset fromDate)
            => await QueryAsync<JobHistoryTable>(ConstQueries.SelectJobHistoryAfter, new { DefinitionId = definitionId, FromDate = fromDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<JobHistoryTable>> GetJobHistory(string definitionId, DateTimeOffset firstDate, DateTimeOffset lastDate)
            => await QueryAsync<JobHistoryTable>(ConstQueries.SelectJobHistoryBetween, new { DefinitionId = definitionId, FirstDate = firstDate.UtcDateTime, LastDate = lastDate.UtcDateTime }).ConfigureAwait(false);

        public async Task<IReadOnlyList<string>> GetActiveJobInstanceIds(string definitionId)
            => await QueryAsync<string>(ConstQueries.SelectJobHistoryActive, new { DefinitionId = definitionId }).ConfigureAwait(false);

        public async Task InsertStatus<TJobTypeProperties>(JobStatus<TJobTypeProperties> status)
        where TJobTypeProperties : class, new()
            => await ExecAsync(ConstQueries.InsertJobHistory, History(status)).ConfigureAwait(false);

        public async Task UpdateStatus<TJobTypeProperties>(JobStatus<TJobTypeProperties> status)
        where TJobTypeProperties : class, new()
            => await ExecAsync(ConstQueries.UpdateJobHistory, History(status)).ConfigureAwait(false);

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

        public JobStatus<TJobTypeProperties> DeserializeDetails<TJobTypeProperties>(JobHistoryTable history)
        where TJobTypeProperties : class, new()
            => JsonConvert.DeserializeObject<JobStatus<TJobTypeProperties>>(history.FullDetailsJson);

        private JobHistoryTable History<TJobTypeProperties>(JobStatus<TJobTypeProperties> status)
        where TJobTypeProperties : class, new()
            => new JobHistoryTable
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
