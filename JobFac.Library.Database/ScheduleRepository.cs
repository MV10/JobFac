using JobFac.Library.DataModels;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Library.Database
{
    public class ScheduleRepository : DapperRepositoryBase
    {
        public ScheduleRepository(IConfiguration config)
            : base(config)
        { }

        public async Task<List<PendingScheduledJob>> GetPendingJobs(long onOrBeforeScheduleTarget)
            => await QueryMutableAsync<PendingScheduledJob>(ConstQueries.SelectPendingScheduledJobs, new { ScheduleTarget = onOrBeforeScheduleTarget }).ConfigureAwait(false);

        public async Task<string> GetJobActivationValue(string definitionId, long scheduleTarget)
            => await QueryScalarAsync<string>(ConstQueries.SelectScheduledJobActivation, new { DefinitionId = definitionId, ScheduleTarget = scheduleTarget }).ConfigureAwait(false);

        public async Task UpdatedActivationValue(string definitionId, long scheduleTarget, string activationValue)
            => await ExecAsync(ConstQueries.UpdateScheduledJobActivation, new { DefinitionId = definitionId, ScheduleTarget = scheduleTarget, Activation = activationValue }).ConfigureAwait(false);

        public async Task DeletePendingScheduledJobs(string definitionId)
            => await ExecAsync(ConstQueries.DeletePendingScheduledJobs, new { DefinitionId = definitionId }).ConfigureAwait(false);

        public async Task<IReadOnlyList<JobsWithSchedulesQuery>> GetJobsWithScheduleSettings()
            => await QueryAsync<JobsWithSchedulesQuery>(ConstQueries.SelectJobsWithScheduleSettings, null).ConfigureAwait(false);

        public async Task BulkInsertScheduledJobs(IReadOnlyList<string> scheduleInsertStatements)
            => await ExecAsync(scheduleInsertStatements).ConfigureAwait(false);
    }
}
