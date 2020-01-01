using JobFac.Library;
using JobFac.Library.Database;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
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
    public class ScheduleWriter : Grain, IScheduleWriter
    {
        private readonly ILogger logger;
        private readonly ScheduleRepository scheduleRepository;
        private readonly ConfigRepository configRepository;
        private readonly ConfigCacheService configCache;

        public ScheduleWriter(
            ILoggerFactory loggerFactory,
            ScheduleRepository scheduleRepo,
            ConfigRepository configRepo,
            ConfigCacheService cache)
        {
            logger = loggerFactory.CreateLogger(ConstLogging.JobFacCategoryScheduleWriter);
            scheduleRepository = scheduleRepo;
            configRepository = configRepo;
            configCache = cache;
        }

        // Must execute WasLastRunToday immediately to set these properly.
        private Instant today;
        private ZonedDateTime todayUtc;

        public async Task WriteNewScheduleTargets()
        {
            // Decisions about whether the once-daily run target is near enough:

            if (await WasLastRunToday()) return;

            // This will be an HHmm value such as 1030 or 2200; SchedulerQueue only invokes this
            // grain at 15-minute intervals, so quit unless we're near or past the run-at time.
            var paddedUtc = todayUtc.PlusMinutes(5);
            var paddedTime = paddedUtc.TimeOfDay;
            var runTargetUtc = await configCache.GetValue(ConstConfigKeys.ScheduleWriterRunTargetUtc);
            var (runHour, runMinute) = TargetPlanner.GetHourMinute(runTargetUtc);
            if (paddedUtc.Date == todayUtc.Date && (paddedTime.Hour < runHour || (paddedTime.Hour == runHour && paddedTime.Minute < runMinute))) return;

            // Decision made: yes, write the next day's schedule records.
            logger.LogInformation($"WriteNewScheduleTargets");

            var jobs = await scheduleRepository.GetJobsWithScheduleSettings();
            if (jobs.Count == 0) return;

            foreach (var job in jobs)
            {
                try
                {
                    var planner = new TargetPlanner(job, today.Plus(Duration.FromDays(1)), logger);
                    var newSchedules = planner.GetSchedules();

                    if (newSchedules.Count > 0)
                        await InsertScheduleRows(newSchedules);

                    newSchedules = null;
                }
                catch
                { }
            }

            await configRepository.UpdateConfig(ConstConfigKeys.ScheduleWriterLastRunDateUtc, InstantPattern.General.Format(today));
        }

        public async Task UpdateJobSchedules(string jobDefinitionId, bool removeExistingRows = true)
        {
            logger.LogInformation($"UpdateJobSchedules for job {jobDefinitionId}");

            bool wasLastRunToday = await WasLastRunToday();

            if (removeExistingRows)
                await scheduleRepository.DeletePendingScheduledJobs(jobDefinitionId);

            var job = await scheduleRepository.GetJobScheduleSettings(jobDefinitionId);
            try
            {
                var planner = new TargetPlanner(job, today, logger);
                var newSchedules = planner.GetSchedules();

                // If we've already set up tomorrow's schedules (for all jobs)
                // do a second pass for this job targeting tomorrow's date.
                if (wasLastRunToday)
                {
                    planner = new TargetPlanner(job, today.Plus(Duration.FromDays(1)), logger);
                    newSchedules.AddRange(planner.GetSchedules());
                }

                if (newSchedules.Count > 0)
                    await InsertScheduleRows(newSchedules);

                newSchedules = null;
            }
            catch
            { }
        }

        private async Task<bool> WasLastRunToday()
        {
            today = SystemClock.Instance.GetCurrentInstant();
            todayUtc = today.InUtc();

            Instant lastRun = Instant.MinValue;
            try
            {
                // ConfigCache updates every 5 minutes (see JobFac.Library.ConstTimeouts)
                // so it will always be up-to-date since ScheduleWriter is invoked on a
                // 15-minute cycle (also defined in ConstTimeouts).
                var lastRunTimestamp = await configCache.GetValue(ConstConfigKeys.ScheduleWriterLastRunDateUtc);
                lastRun = InstantPattern.General.Parse(lastRunTimestamp).GetValueOrThrow();
            }
            catch
            { }

            return (todayUtc.Date == lastRun.InUtc().Date);
        }

        private async Task InsertScheduleRows(List<ScheduledJobsTable> rows)
        {
            // The use of string.Format is not a SQL injection risk since the input values
            // are not from external sources (the application supplies the input values and
            // the user can only create definition Ids with a short list of safe characters).
            logger.LogInformation($"Created {rows.Count} new schedule targets");

            int batchSize = 1000;
            int batchCount = (int)Math.Ceiling((double)rows.Count / batchSize);
            List<string> sqlBatches = new List<string>(batchCount);
            for (int i = 0; i < batchCount; i++)
            {
                var subset = rows.Skip(i * batchSize).Take(batchSize);
                var values = subset.Select(s => string.Format(ConstQueries.FragmentScheduledJobsBatchValues, s.DefinitionId, s.ScheduleTarget));
                sqlBatches.Add(ConstQueries.FragmentScheduledJobsBatchInsert + string.Join(",", values));
            }

            await scheduleRepository.BulkInsertScheduledJobs(sqlBatches);
        }
    }
}
