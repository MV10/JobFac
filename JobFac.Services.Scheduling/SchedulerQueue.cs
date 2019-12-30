using JobFac.Library;
using JobFac.Library.Database;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class SchedulerQueue : Grain, ISchedulerQueue
    {
        private readonly ILogger logger;
        private readonly ConfigCacheService configCache;
        private readonly ScheduleRepository scheduleRepository;

        public SchedulerQueue(
            ILoggerFactory loggerFactory, 
            ConfigCacheService configCache,
            ScheduleRepository scheduleRepository)
        {
            logger = loggerFactory.CreateLogger(ConstLogging.JobFacCategorySchedulerQueue);
            this.scheduleRepository = scheduleRepository;
            this.configCache = configCache;
        }

        // How far into the future to cache; cache is only reloaded when fully drained.
        private int cachePeriodMinutes;

        // Largest list in response to GetAssignedJobs call.
        private int maxJobAssignment;

        // This will be a 24-hour-clock value like 1030 or 2200.
        private int scheduleWriterRunTarget;

        private List<JobAssignment> queuedJobs = new List<JobAssignment>();
        private List<JobAssignment> recentAssignments = new List<JobAssignment>();
        private Dictionary<string, DateTimeOffset> knownServices = new Dictionary<string, DateTimeOffset>();
        private IReadOnlyList<JobAssignment> emptyList = new List<JobAssignment>(0);

        public override async Task OnActivateAsync()
        {
            cachePeriodMinutes = int.Parse(await configCache.GetValue(ConstConfigKeys.SchedulerQueueCachePeriodMinutes));
            maxJobAssignment = int.Parse(await configCache.GetValue(ConstConfigKeys.SchedulerQueueMaxJobAssignment));
            scheduleWriterRunTarget = int.Parse(await configCache.GetValue(ConstConfigKeys.ScheduleWriterRunTarget));

            //await RefreshQueuedJobs();

            // TODO set 15 minute ScheduleWriter timer

            await base.OnActivateAsync();
        }

        public Task SchedulerServiceStarting(string schedulerServiceId)
        {
            logger.LogInformation($"SchedulerServiceStarting: {schedulerServiceId}");
            knownServices.AddOrUpdate(schedulerServiceId, DateTimeOffset.Now);
            return Task.CompletedTask;
        }

        public Task SchedulerServiceStopping(string schedulerServiceId)
        {
            logger.LogInformation($"SchedulerServiceStopping: {schedulerServiceId}");
            knownServices.Remove(schedulerServiceId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<JobAssignment>> GetJobAssignments(string schedulerServiceId)
        {
            throw new NotImplementedException();
        }

        //private async Task RefreshQueuedJobs()
        //{

        //}
    }
}
