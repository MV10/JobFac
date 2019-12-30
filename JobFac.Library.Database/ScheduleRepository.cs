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
    }
}
