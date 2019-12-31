using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// TODO ensure scheduler system uses UTC everywhere
// TODO consider local-to-UTC "tomorrow" issue in SchedulerQueue's call to ScheduleWriter
// TODO handle local-to-UTC conversions in ScheduleWriter

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

        private int today;
        private List<ScheduledJobsTable> newSchedules;

        // Date-based information about the schedule target; used to determine
        // whether the job should be scheduled for the date in question. Set
        // targetDate, then call CalculateScheduleTargetValues to set the rest.
        private DateTimeOffset targetDate;
        private int targetMonth;
        private int targetDay;
        private int targetLastDayOfMonth;
        private string targetDayOfWeek;
        private string targetDayOfMonth;
        private string targetMonthAndDay; // mm/dd
        private bool targetIsFirstDayOfMonth;
        private bool targetIsLastDayOfMonth;
        private bool targetIsFirstWeekdayOfMonth;
        private bool targetIsLastWeekdayOfMonth;

        public async Task WriteNewScheduleTargets()
        {
            if (!await ShouldExecute()) return;
            logger.LogInformation($"WriteNewScheduleTargets");
            await WriteSchedulesForTomorrow();
            await configRepository.UpdateConfig(ConstConfigKeys.ScheduleWriterLastRunDate, today.ToString());
        }

        public async Task UpdateJobSchedules(string jobDefinitionId, bool removeExistingRows = true)
        {
            logger.LogInformation($"UpdateJobSchedules for job {jobDefinitionId}");

            if (removeExistingRows)
                await scheduleRepository.DeletePendingScheduledJobs(jobDefinitionId);

            // TODO complete UpdateJobSchedules
            throw new NotImplementedException();
        }

        private async Task<bool> ShouldExecute()
        {
            // ConfigCache updates every 5 minutes (see JobFac.Library.ConstTimeouts)
            // so it will always be up-to-date since ScheduleWriter is invoked on a
            // 15-minute cycle (also defined in ConstTimeouts).

            // This will be a date-only yyyyMMdd value such as 20191225
            int lastRun = int.Parse(await configCache.GetValue(ConstConfigKeys.ScheduleWriterLastRunDate));
            today = int.Parse(DateTimeOffset.UtcNow.ToString("yyyyMMdd"));
            if (today == lastRun) return false;

            // This will be an HHmm value such as 1030 or 2200; SchedulerQueue only invokes this
            // method at 15-minute intervals, so quit unless we're close to the runAt time (or past it).
            int runAt = int.Parse(await configCache.GetValue(ConstConfigKeys.ScheduleWriterRunTarget));
            int now = int.Parse(DateTimeOffset.UtcNow.AddMinutes(5).ToString("HHmm"));
            if (now < runAt) return false;

            return true;
        }

        private async Task WriteSchedulesForTomorrow()
        {
            var jobs = await scheduleRepository.GetJobsWithScheduleSettings();
            if (jobs.Count == 0) return;

            targetDate = DateTimeOffset.UtcNow.Date.AddDays(1);
            CalculateScheduleTargetValues();

            newSchedules = new List<ScheduledJobsTable>();
            foreach(var job in jobs)
            {
                // It's safe to assume the data returned by the query is always valid. The
                // query excludes any ScheduleDateMode.Unscheduled records, and the validator
                // for BaseDefinition ensures all of the settings and combinations are correct.
                var dates = job.ScheduleDates.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

                switch(job.ScheduleDateMode)
                {
                    // any of 0-6 with commas
                    case ScheduleDateMode.DaysOfWeek:
                        if (dates.Any(d => d.Equals(targetDayOfWeek))) CreateSchedulesForJob(job);
                        break;

                    // any numeric with commas, or first,last
                    case ScheduleDateMode.DaysOfMonth:
                        if (dates.Any(d => d.Equals(targetDayOfMonth))
                            || (targetIsFirstDayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
                            || (targetIsLastDayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase))))
                            CreateSchedulesForJob(job);
                        break;

                    // mm/dd,mm/dd,mm/dd
                    case ScheduleDateMode.SpecificDates:
                        if (dates.Any(d => d.Equals(targetMonthAndDay))) CreateSchedulesForJob(job);
                        break;

                    // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
                    case ScheduleDateMode.DateRanges:
                        if(dates.Any(d => InDateRange(d))) CreateSchedulesForJob(job);
                        break;

                    // first,last
                    case ScheduleDateMode.WeekdaysOfMonth:
                        if ((targetIsFirstWeekdayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
                            || (targetIsLastWeekdayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase))))
                            CreateSchedulesForJob(job);
                        break;
                }
            }

            if (newSchedules.Count > 0) 
                await WriteScheduleRows();

            logger.LogInformation($"Created {newSchedules.Count} new schedule targets");
            newSchedules = null;
        }

        private async Task WriteScheduleRows()
        {
            // The use of string.Format is not a SQL injection risk since the input values
            // are not from external sources (the application supplies the input values and
            // the user can only create definition Ids with a short list of safe characters).

            int batchSize = 1000;
            int batchCount = (int)Math.Ceiling((double)newSchedules.Count / batchSize);
            List<string> sqlBatches = new List<string>(batchCount);
            for (int i = 0; i < batchCount; i++)
            {
                var subset = newSchedules.Skip(i * batchSize).Take(batchSize);
                var values = subset.Select(s => string.Format(ConstQueries.FragmentScheduledJobsBatchValues, s.DefinitionId, s.ScheduleTarget));
                sqlBatches.Add(ConstQueries.FragmentScheduledJobsBatchInsert + string.Join(",", values));
            }

            await scheduleRepository.BulkInsertScheduledJobs(sqlBatches);
        }

        // afterCurrentTime is used when updating a specific job's schedule when targetDate is set to today's
        // date (which happens when schedules for tomorrow haven't been written yet)
        private void CreateSchedulesForJob(JobsWithSchedulesQuery job, bool afterCurrentTime = false)
        {
            var times = job.ScheduleTimes.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            switch(job.ScheduleTimeMode)
            {
                // every hour at the indicated minutes (eg. 00,15,30,45)
                case ScheduleTimeMode.Minutes:
                    for(int hour = 0; hour < 24; hour++)
                    {
                        foreach(var minute in times)
                        {
                            var target = new DateTimeOffset(targetDate.Year, targetDay, targetMonth, hour, int.Parse(minute), 0, TimeSpan.Zero);
                            CreateScheduleEntry(target, job.Id);
                        }
                    }
                    break;

                // specific times: 1130,1400,2245
                case ScheduleTimeMode.HoursMinutes:
                    foreach(var time in times)
                    {
                        int n = time.Length - 2;
                        int hour = int.Parse(time.Substring(0, n));
                        int minute = int.Parse(time.Substring(n, 2));
                        var target = new DateTimeOffset(targetDate.Year, targetDay, targetMonth, hour, minute, 0, TimeSpan.Zero);
                        CreateScheduleEntry(target, job.Id);
                    }
                    break;

                // a single value, every N minutes starting after midnight (eg. 30)
                case ScheduleTimeMode.Interval:
                    for (int hour = 0; hour < 24; hour++)
                    {
                        int interval = int.Parse(times[0]);
                        for(int minute = 0; minute < 59; minute += interval)
                        {
                            var target = new DateTimeOffset(targetDate.Year, targetDay, targetMonth, hour, minute, 0, TimeSpan.Zero);
                            CreateScheduleEntry(target, job.Id);
                        }
                    }
                    break;
            }
        }

        // Set the targetDate field before calling this.
        private void CalculateScheduleTargetValues()
        {
            targetLastDayOfMonth = (targetDate.AddMonths(1).AddDays(-1).Day);
            targetMonth = targetDate.Month;
            targetDay = targetDate.Day;
            targetDayOfWeek = ((int)targetDate.DayOfWeek).ToString();
            targetDayOfMonth = targetDay.ToString();
            targetMonthAndDay = targetDate.ToString("MM/dd");
            targetIsFirstDayOfMonth = targetDay == 1;
            targetIsLastDayOfMonth = targetDay == targetLastDayOfMonth;
            targetIsFirstWeekdayOfMonth = targetDay == FirstWeekdayOfMonth();
            targetIsLastWeekdayOfMonth = targetDay == LastWeekdayOfMonth();
        }

        private int FirstWeekdayOfMonth()
        {
            var result = new DateTimeOffset(targetDate.Year, targetDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result.AddDays(1);
            return result.Day;
        }

        private int LastWeekdayOfMonth()
        {
            var result = new DateTimeOffset(targetDate.Year, targetDate.Month, targetLastDayOfMonth, 0, 0, 0, TimeSpan.Zero);
            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
                result.AddDays(-1);
            return result.Day;
        }

        // For ScheduleDateMode.DateRanges the ScheduleDates values should be date ranges in the
        // format mm/dd-mm/dd (exactly that format); this determines if targetDate is in that
        // range (inclusive of the start/end dates for the range).
        private bool InDateRange(string range)
        {
            var m1 = int.Parse(range.Substring(0, 2));
            var d1 = int.Parse(range.Substring(3, 2));
            var m2 = int.Parse(range.Substring(6, 2));
            var d2 = int.Parse(range.Substring(9, 2));
            return ((targetMonth > m1 || (targetMonth == m1 && targetDay >= d1)) && (targetMonth < m2 || (targetMonth == m2 && targetDay <= d2)));
        }

        private void CreateScheduleEntry(DateTimeOffset scheduleTarget, string jobDefinitionId)
        {
            newSchedules.Add(new ScheduledJobsTable
            {
                DefinitionId = jobDefinitionId,
                ScheduleTarget = Formatting.ScheduleTargetTimestamp(scheduleTarget)
            });
        }
    }
}
