using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Logging;
using NodaTime;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

namespace JobFac.Services.Scheduling
{
    // IClusterSingletonGrain (auto-assigned integer key 0)
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

        // Tracks when a given SchedulerService was last seen; count determines how many jobs are assigned by GetAssignedJobs.
        private Dictionary<string, Instant> knownServices = new Dictionary<string, Instant>();
        
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
            knownServices.AddOrUpdate(schedulerServiceId, SystemClock.Instance.GetCurrentInstant());
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

            var now = SystemClock.Instance.GetCurrentInstant();
            var nowUtc = now.InUtc();

            knownServices.AddOrUpdate(schedulerServiceId, now);
            EvictDeadServices();

            var currentMinute = nowUtc.Minute;

            if (currentMinute % ConstTimeouts.TryInvokeScheduleWriterMinutes == 0)
                RunScheduleWriter();

            if (queuedJobs.Count == 0 || currentMinute != jobsQueuedForMinute)
            {
                if (currentMinute == jobsQueuedForMinute) return queuedJobs; // empty queue
                queuedJobs = await scheduleRepository.GetPendingJobs(nowUtc.ToDateTimeOffset());
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
            var now = SystemClock.Instance.GetCurrentInstant();
            var nowUtc = now.InUtc();

            var currentMinute = nowUtc.Minute;
            if (currentMinute == lastServiceEvictionMinute) return;
            lastServiceEvictionMinute = currentMinute;

            var threshold = now.Plus(Duration.FromMinutes(-5));
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
