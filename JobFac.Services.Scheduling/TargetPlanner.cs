using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Linq;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

//  This class interprets user-input as relating to a given time zone (see
//  list here: https://nodatime.org/TimeZones). They will be resolved to a
//  a Noda Time Instant then converted to a UTC DateTimeOffset for storage.

namespace JobFac.Services.Scheduling
{
    public class TargetPlanner
    {
        private readonly JobsWithSchedulesQuery job;
        private readonly DateTimeAnalysis analysis;
        private readonly ILogger logger;
        private readonly Instant targetDate;
        private readonly DateTimeZone jobZone;

        public TargetPlanner(
            JobsWithSchedulesQuery job, 
            Instant forTargetDate, 
            ILogger parentLogger = null)
        {
            this.job = job;
            targetDate = forTargetDate;
            logger = parentLogger;

            jobZone = DateTimeZoneProviders.Tzdb[job.ScheduleTimeZone];
            var zonedDate = forTargetDate.InZone(jobZone).Date;
            analysis = DateTimeAnalysis.ForZonedTargetDate(zonedDate);
        }

        private List<ScheduledJobsTable> newSchedules = new List<ScheduledJobsTable>();

        public List<ScheduledJobsTable> GetSchedules()
        {
            newSchedules.Clear();
            EvaluateJobForScheduling();
            return newSchedules;
        }

        private void EvaluateJobForScheduling()
        {
            // It's safe to assume the data returned by the ScheduleWriter query is valid.
            // The query excludes ScheduleDateMode.Unscheduled records, and the validator
            // for BaseDefinition ensures all of the settings and combinations are correct.

            var dates = Formatting.SplitCommaSeparatedList(job.ScheduleDates);

            switch (job.ScheduleDateMode)
            {
                // any of 1-7 with commas, ISO format (Monday = 1, Sunday = 7)
                case ScheduleDateMode.DaysOfWeek:
                    if (analysis.InDaysOfWeek(dates)) CreateSchedulesForDate();
                    break;

                // any numeric with commas, or first,last
                case ScheduleDateMode.DaysOfMonth:
                    if (analysis.InDaysOfMonth(dates)) CreateSchedulesForDate();
                    break;

                // mm/dd,mm/dd,mm/dd
                case ScheduleDateMode.SpecificDates:
                    if (analysis.InSpecificDates(dates)) CreateSchedulesForDate();
                    break;

                // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
                case ScheduleDateMode.DateRanges:
                    if (analysis.InDateRanges(dates)) CreateSchedulesForDate();
                    break;

                // first,last,weekday,weekend
                case ScheduleDateMode.Weekdays:
                    if (analysis.InWeekdays(dates)) CreateSchedulesForDate();
                    break;
            }
        }

        private void CreateSchedulesForDate()
        {
            var todayUtc = SystemClock.Instance.GetCurrentInstant().InUtc();
            var targetDateIsToday = (todayUtc.Date == targetDate.InUtc().Date);

            // When the schedules are being created for the current date, don't
            // create new schedules targeting times which have already passed.
            var filterHour = todayUtc.Hour;
            var filterMinute = todayUtc.Minute;

            var times = Formatting.SplitCommaSeparatedList(job.ScheduleTimes);

            switch (job.ScheduleTimeMode)
            {
                // every hour at the indicated minutes (eg. 00,15,30,45)
                case ScheduleTimeMode.Minutes:
                    for (int hour = 0; hour < 24; hour++)
                    {
                        foreach (var min in times)
                        {
                            var minute = int.Parse(min);
                            TryAddUtcEntry(hour, minute);
                        }
                    }
                    break;

                // specific times: 1130,1400,2245
                case ScheduleTimeMode.HoursMinutes:
                    foreach (var time in times)
                    {
                        var (hour, minute) = DateTimeAnalysis.GetHourMinute(time);
                        TryAddUtcEntry(hour, minute);
                    }
                    break;

                // a single value, every N minutes starting after midnight (eg. 30)
                case ScheduleTimeMode.Interval:
                    for (int hour = 0; hour < 24; hour++)
                    {
                        int interval = int.Parse(times[0]);
                        for (int minute = 0; minute < 59; minute += interval)
                        {
                            TryAddUtcEntry(hour, minute);
                        }
                    }
                    break;
            }

            void TryAddUtcEntry(int hour, int minute)
            {
                var local = new LocalDateTime(analysis.Date.Year, analysis.Month, analysis.Day, hour, minute);
                try
                {
                    var scheduleTarget = local
                                         .InZoneStrictly(jobZone)    // TODO make InZoneStrictly vs Leniently a config option
                                         .WithZone(DateTimeZone.Utc)
                                         .ToDateTimeOffset();

                    if (!targetDateIsToday || filterHour < scheduleTarget.Hour || (filterHour == scheduleTarget.Hour && filterMinute <= scheduleTarget.Minute))
                    {
                        newSchedules.Add(new ScheduledJobsTable
                        {
                            DefinitionId = job.Id,
                            ScheduleTarget = scheduleTarget
                        });
                    }
                }
                catch(Exception ex) 
                when (ex is SkippedTimeException || ex is AmbiguousTimeException)
                {
                    logger?.LogWarning($"Skipping invalid/ambiguous results applying timezone {jobZone.Id} to {LocalDateTimePattern.GeneralIso.Format(local)}");
                }
            }
        }
    }
}
