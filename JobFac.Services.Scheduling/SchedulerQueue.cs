using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class SchedulerQueue : Grain, ISchedulerQueue
    {
        private readonly ILogger logger;
        private readonly ConfigCacheService configCache;
        private readonly ScheduleRepository scheduleRepository;
        private readonly IScheduleWriter scheduleWriter;

        public SchedulerQueue(
            ILoggerFactory loggerFactory, 
            ConfigCacheService configCache,
            ScheduleRepository scheduleRepository)
        {
            logger = loggerFactory.CreateLogger(ConstLogging.JobFacCategorySchedulerQueue);
            this.scheduleRepository = scheduleRepository;
            this.configCache = configCache;
            scheduleWriter = GrainFactory.GetGrain<IScheduleWriter>();
        }

        // The count determines how many jobs are assigned to each call to GetAssignedJobs.
        private Dictionary<string, DateTimeOffset> knownServices = new Dictionary<string, DateTimeOffset>();
        
        // Only evict expired services once per minute
        private int lastServiceEvictionMinute = -1;

        // Largest allowable list returned by the GetAssignedJobs call.
        private int maxJobAssignment;

        // A 24-hour-clock value like 1030 or 2200, we invoke the ScheduleWriter every 15 minutes.
        private int lastScheduleWriterRun = -1;

        // Jobs not started this minute or any previous minute
        private List<PendingScheduledJob> queuedJobs = new List<PendingScheduledJob>();

        // What minute jobs have been queued and served up for (refreshes if we hit a new minute)
        private int jobsQueuedForMinute = -1;
        
        public override async Task OnActivateAsync()
        {
            maxJobAssignment = int.Parse(await configCache.GetValue(ConstConfigKeys.SchedulerQueueMaxJobAssignment));

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

        public async Task<IReadOnlyList<PendingScheduledJob>> GetJobAssignments(string schedulerServiceId)
        {
            logger.LogInformation($"GetJobAssignments: {schedulerServiceId}");
            knownServices.AddOrUpdate(schedulerServiceId, DateTimeOffset.Now);
            EvictDeadServices();

            var current = DateTimeOffset.Now.Minute;

            if (current % 15 == 0)
                RunScheduleWriter();

            if (queuedJobs.Count == 0 || current != jobsQueuedForMinute)
            {
                if (current == jobsQueuedForMinute) return queuedJobs; // empty queue
                jobsQueuedForMinute = current;
                var thisMinute = Formatting.ScheduleTargetTimestamp(DateTimeOffset.Now);
                queuedJobs = await scheduleRepository.GetPendingJobs(thisMinute);
                if (queuedJobs.Count == 0) return queuedJobs; // still an empty queue
            }

            var maxToAssign = Math.Min(maxJobAssignment, (queuedJobs.Count / knownServices.Count) + 1);
            IReadOnlyList<PendingScheduledJob> assignment = queuedJobs.Take(maxToAssign).ToList();
            queuedJobs.RemoveRange(0, assignment.Count);
            logger.LogInformation($"Assigned {assignment.Count} scheduled jobs");
            return assignment;
        }

        private void EvictDeadServices()
        {
            var current = DateTimeOffset.Now.Minute;
            if (current == lastServiceEvictionMinute) return;
            lastServiceEvictionMinute = current;

            var threshold = DateTimeOffset.Now.AddMinutes(-5);
            var expired = knownServices.Where(kvp => kvp.Value <= threshold).ToList();
            foreach (var kvp in expired) knownServices.Remove(kvp.Key);
        }

        private void RunScheduleWriter()
        {
            int runtime = int.Parse(DateTimeOffset.Now.ToString("HHmm"));
            if (runtime != lastScheduleWriterRun)
            {
                lastScheduleWriterRun = runtime;
                scheduleWriter.WriteNewScheduleTargets().Ignore();
            }
        }
    }
}
