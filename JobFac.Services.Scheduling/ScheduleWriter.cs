using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
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
//  This class can interpret user-input as relating to a given time zone (see
//  list here: https://nodatime.org/TimeZones). They will be resolved to a
//  a Noda Time Instant then converted to a UTC DateTimeOffset for storage.
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

        private Instant today;
        private ZonedDateTime todayUtc;
        private List<ScheduledJobsTable> newSchedules;
        DateTimeZone utcTimeZone = DateTimeZoneProviders.Tzdb["Etc/UTC"];

        public async Task WriteNewScheduleTargets()
        {
            today = SystemClock.Instance.GetCurrentInstant();
            todayUtc = today.InUtc();

            // Decision about whether the once-daily run target is near enough:

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
            if (todayUtc.Date == lastRun.InUtc().Date) return;

            // This will be an HHmm value such as 1030 or 2200; SchedulerQueue only invokes this
            // grain at 15-minute intervals, so quit unless we're near or past the run-at time.
            var paddedUtc = todayUtc.PlusMinutes(5);
            var paddedTime = paddedUtc.TimeOfDay;
            var runTargetUtc = await configCache.GetValue(ConstConfigKeys.ScheduleWriterRunTargetUtc);
            var (runHour, runMinute) = GetHourMinute(runTargetUtc);
            if (paddedUtc.Date == todayUtc.Date && (paddedTime.Hour < runHour || (paddedTime.Hour == runHour && paddedTime.Minute < runMinute))) return;

            // Decision made: yes, write the next day's schedule records.
            logger.LogInformation($"WriteNewScheduleTargets");

            var jobs = await scheduleRepository.GetJobsWithScheduleSettings();
            if (jobs.Count == 0) return;

            newSchedules = new List<ScheduledJobsTable>();
            foreach (var job in jobs)
            {
                try
                {
                    var jobDataTimeZone = DateTimeZoneProviders.Tzdb[job.ScheduleTimeZone];
                    targetDate = today.InZone(jobDataTimeZone).Plus(Duration.FromDays(1)).Date;
                    targetDateIsToday = true;
                    CalculateScheduleTargetValues();
                    EvaluateJobForScheduling(job);
                }
                catch
                { }
            }

            if (newSchedules.Count > 0)
                await InsertScheduleRows();

            logger.LogInformation($"Created {newSchedules.Count} new schedule targets");
            newSchedules = null;

            await configRepository.UpdateConfig(ConstConfigKeys.ScheduleWriterLastRunDateUtc, InstantPattern.General.Format(today));
        }

        public async Task UpdateJobSchedules(string jobDefinitionId, bool removeExistingRows = true)
        {
            logger.LogInformation($"UpdateJobSchedules for job {jobDefinitionId}");

            if (removeExistingRows)
                await scheduleRepository.DeletePendingScheduledJobs(jobDefinitionId);

            newSchedules = new List<ScheduledJobsTable>();
            try
            {
                var job = await scheduleRepository.GetJobScheduleSettings(jobDefinitionId);
                var jobDataTimeZone = DateTimeZoneProviders.Tzdb[job.ScheduleTimeZone];
                targetDate = today.InZone(jobDataTimeZone).Date;
                targetDateIsToday = false;
                CalculateScheduleTargetValues();
                EvaluateJobForScheduling(job);
            }
            catch
            { }

            if (newSchedules.Count > 0)
                await InsertScheduleRows();

            logger.LogInformation($"Created {newSchedules.Count} new schedule targets");
            newSchedules = null;
        }

        // Date-based information about the schedule target; used to determine
        // whether the job should be scheduled for the date in question. Set
        // targetDate, then call CalculateScheduleTargetValues to set the rest.
        private LocalDate targetDate; // adjusted to ScheduleTimeZone in JobDefinition
        private bool targetDateIsToday;
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

        // Set the targetDate field before calling this.
        private void CalculateScheduleTargetValues()
        {
            targetLastDayOfMonth = targetDate.With(DateAdjusters.EndOfMonth).Day;
            targetMonth = targetDate.Month;
            targetDay = targetDate.Day;
            targetDayOfWeek = ((int)targetDate.DayOfWeek).ToString();
            targetDayOfMonth = targetDay.ToString();
            targetMonthAndDay = $"{targetDate:MM/dd}";
            targetIsFirstDayOfMonth = targetDay == 1;
            targetIsLastDayOfMonth = targetDay == targetLastDayOfMonth;

            targetIsFirstWeekdayOfMonth = targetDay == FirstWeekdayOfMonth();
            targetIsLastWeekdayOfMonth = targetDay == LastWeekdayOfMonth();
        }

        private int FirstWeekdayOfMonth()
        {
            var result = targetDate.With(DateAdjusters.StartOfMonth);
            while (result.DayOfWeek == IsoDayOfWeek.Saturday || result.DayOfWeek == IsoDayOfWeek.Sunday)
                result.PlusDays(1);
            return result.Day;
        }

        private int LastWeekdayOfMonth()
        {
            var result = targetDate.With(DateAdjusters.EndOfMonth);
            while (result.DayOfWeek == IsoDayOfWeek.Saturday || result.DayOfWeek == IsoDayOfWeek.Sunday)
                result.PlusDays(-1);
            return result.Day;
        }

        private void EvaluateJobForScheduling(JobsWithSchedulesQuery job)
        {
            // It's safe to assume the data returned by the query is always valid. The
            // query excludes any ScheduleDateMode.Unscheduled records, and the validator
            // for BaseDefinition ensures all of the settings and combinations are correct.
            var dates = job.ScheduleDates.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            switch (job.ScheduleDateMode)
            {
                // any of 1-7 with commas, ISO format (Monday = 1, Sunday = 7)
                case ScheduleDateMode.DaysOfWeek:
                    if (dates.Any(d => d.Equals(targetDayOfWeek))) CreateSchedulesForDate(job);
                    break;

                // any numeric with commas, or first,last
                case ScheduleDateMode.DaysOfMonth:
                    if (dates.Any(d => d.Equals(targetDayOfMonth))
                        || (targetIsFirstDayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
                        || (targetIsLastDayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase))))
                        CreateSchedulesForDate(job);
                    break;

                // mm/dd,mm/dd,mm/dd
                case ScheduleDateMode.SpecificDates:
                    if (dates.Any(d => d.Equals(targetMonthAndDay))) CreateSchedulesForDate(job);
                    break;

                // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
                case ScheduleDateMode.DateRanges:
                    if (dates.Any(d => InDateRange(d))) CreateSchedulesForDate(job);
                    break;

                // first,last
                case ScheduleDateMode.WeekdaysOfMonth:
                    if ((targetIsFirstWeekdayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
                        || (targetIsLastWeekdayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase))))
                        CreateSchedulesForDate(job);
                    break;
            }
        }

        private void CreateSchedulesForDate(JobsWithSchedulesQuery job)
        {
            // When the schedules are being created for the current date, don't
            // create new schedules targeting times which have already passed.
            var filterHour = todayUtc.Hour;
            var filterMinute = todayUtc.Minute;

            var jobDataTimeZone = DateTimeZoneProviders.Tzdb[job.ScheduleTimeZone];

            var times = job.ScheduleTimes.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            switch (job.ScheduleTimeMode)
            {
                // every hour at the indicated minutes (eg. 00,15,30,45)
                case ScheduleTimeMode.Minutes:
                    for(int hour = 0; hour < 24; hour++)
                    {
                        foreach(var min in times)
                        {
                            var minute = int.Parse(min);
                            TryAddEntry(hour, minute);
                        }
                    }
                    break;

                // specific times: 1130,1400,2245
                case ScheduleTimeMode.HoursMinutes:
                    foreach(var time in times)
                    {
                        var (hour, minute) = GetHourMinute(time);
                        TryAddEntry(hour, minute);
                    }
                    break;

                // a single value, every N minutes starting after midnight (eg. 30)
                case ScheduleTimeMode.Interval:
                    for (int hour = 0; hour < 24; hour++)
                    {
                        int interval = int.Parse(times[0]);
                        for(int minute = 0; minute < 59; minute += interval)
                        {
                            TryAddEntry(hour, minute);
                        }
                    }
                    break;
            }

            void TryAddEntry(int hour, int minute)
            {
                try
                {
                    if (!targetDateIsToday || filterHour < hour || (filterHour == hour && filterMinute <= minute))
                    {
                        newSchedules.Add(new ScheduledJobsTable
                        {
                            DefinitionId = job.Id,
                            ScheduleTarget = new LocalDateTime(targetDate.Year, targetMonth, targetDay, hour, minute)
                                                .InZoneStrictly(jobDataTimeZone)
                                                .WithZone(utcTimeZone)
                                                .ToDateTimeOffset()
                    });
                    }
                }
                catch
                { 
                    // InZoneStrictly throws an exception if the date parameters
                    // are invalid for the requested timezone.
                }
            }
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

        private (int, int) GetHourMinute(string HHmm)
        {
            int n = HHmm.Length - 2;
            int hour = int.Parse(HHmm.Substring(0, n));
            int minute = int.Parse(HHmm.Substring(n, 2));
            return (hour, minute);
        }

        private async Task InsertScheduleRows()
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

    }
}
