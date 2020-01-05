using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Services.Scheduling;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestSchedulePlanner : CoordinatedBackgroundService
    {
        public TestSchedulePlanner(IHostApplicationLifetime appLifetime)
            : base(appLifetime)
        {
        }

        DateTimeZone tz = DateTimeZoneProviders.Tzdb["America/New_York"];

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            try
            {
                var now = SystemClock.Instance.GetCurrentInstant();
                var target = now.Plus(Duration.FromDays(1));

                Console.WriteLine($"Now:    {ZonedDateTimePattern.GeneralFormatOnlyIso.Format(now.InZone(tz))}");
                Console.WriteLine($"Target: {ZonedDateTimePattern.GeneralFormatOnlyIso.Format(target.InZone(tz))}");

                var jobs = GetTestJobs(target);
                foreach (var job in jobs) RunTest(job, target);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex}");
            }
            finally
            {
                Console.WriteLine("Calling StopApplication");
                appLifetime.StopApplication();
            }

        }

        private void RunTest(JobsWithSchedulesQuery job, Instant targetDate)
        {
            Console.WriteLine($"\nJob {job.Id}:\n  Date Mode: {job.ScheduleDateMode} = {job.ScheduleDates}\n  Time Mode: {job.ScheduleTimeMode} = {job.ScheduleTimes}");
            var planner = new TargetPlanner(job, targetDate);
            var results = planner.GetSchedules();
            Console.WriteLine($"  Schedules: {results.Count}");
            int count = 0;
            foreach(var s in results)
            {
                var zoned = ZonedDateTime.FromDateTimeOffset(s.ScheduleTarget).WithZone(tz);
                Console.WriteLine($"  {ZonedDateTimePattern.ExtendedFormatOnlyIso.Format(zoned)}");
                if(++count == 3)
                {
                    if (results.Count > 3) Console.WriteLine($"  ({results.Count - 3} variations omitted)");
                    break;
                }
            }
        }

        private List<JobsWithSchedulesQuery> GetTestJobs(Instant targetDate)
        {
            var now = targetDate.InZone(tz).Date;

            var jobs = new List<JobsWithSchedulesQuery>
            {
                new JobsWithSchedulesQuery
                {
                    Id = "time.Interval",
                    ScheduleDateMode = ScheduleDateMode.DaysOfWeek,
                    ScheduleDates = "1,2,3,4,5,6,7",
                    ScheduleTimeMode = ScheduleTimeMode.Interval,
                    ScheduleTimes = "30"
                },

                new JobsWithSchedulesQuery
                {
                    Id = "time.HoursMinutes",
                    ScheduleDateMode = ScheduleDateMode.DaysOfWeek,
                    ScheduleDates = "1,2,3,4,5,6,7",
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1030,1345,1715"
                },

                new JobsWithSchedulesQuery
                {
                    Id = "time.Minutes",
                    ScheduleDateMode = ScheduleDateMode.DaysOfWeek,
                    ScheduleDates = "1,2,3,4,5,6,7",
                    ScheduleTimeMode = ScheduleTimeMode.Minutes,
                    ScheduleTimes = "20,30,40"
                },
            };

            var fmt = CultureInfo.InvariantCulture.DateTimeFormat;

            AddDaysOfWeekTest();
            AddDaysOfMonthTests();
            AddSpecificDatesTests();
            AddDateRangesTests();
            AddWeekdaysOfMonthTests();

            return jobs;

            void AddDaysOfWeekTest()
            {
                // ScheduleTimeMode tests are all positive DayOfWeek tests.

                var n = new JobsWithSchedulesQuery
                {
                    Id = "date.DaysOfWeek.negative",
                    ScheduleDateMode = ScheduleDateMode.DaysOfWeek,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                n.ScheduleDates = ((int)now.PlusDays(1).DayOfWeek).ToString();

                jobs.Add(n);
            }

            void AddDaysOfMonthTests()
            {
                var p = new JobsWithSchedulesQuery
                {
                    Id = "date.DaysOfMonth.positive",
                    ScheduleDateMode = ScheduleDateMode.DaysOfMonth,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                var n = new JobsWithSchedulesQuery
                {
                    Id = "date.DaysOfMonth.negative",
                    ScheduleDateMode = ScheduleDateMode.DaysOfMonth,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                p.ScheduleDates = now.Day.ToString();
                n.ScheduleDates = now.PlusDays(1).Day.ToString();

                jobs.Add(p);
                jobs.Add(n);
            }

            void AddSpecificDatesTests()
            {
                var p = new JobsWithSchedulesQuery
                {
                    Id = "date.SpecificDates.positive",
                    ScheduleDateMode = ScheduleDateMode.SpecificDates,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                var n = new JobsWithSchedulesQuery
                {
                    Id = "date.SpecificDates.negative",
                    ScheduleDateMode = ScheduleDateMode.SpecificDates,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                p.ScheduleDates = now.ToString("MM/dd", fmt);
                n.ScheduleDates = now.PlusDays(1).ToString("MM/dd", fmt);

                jobs.Add(p);
                jobs.Add(n);
            }

            void AddDateRangesTests()
            {
                var p = new JobsWithSchedulesQuery
                {
                    Id = "date.DateRanges.positive",
                    ScheduleDateMode = ScheduleDateMode.DateRanges,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                var n = new JobsWithSchedulesQuery
                {
                    Id = "date.DateRanges.negative",
                    ScheduleDateMode = ScheduleDateMode.DateRanges,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                // full year crossing the year boundary; only one of these needs to
                // match, so swap the order to check each variation on the rule
                p.ScheduleDates = "06/15-06/14,01/01-12/31";

                n.ScheduleDates = now.PlusMonths(-2).ToString("MM/dd", fmt) + "-" + now.PlusMonths(-1).ToString("MM/dd", fmt);

                jobs.Add(p);
                jobs.Add(n);
            }

            void AddWeekdaysOfMonthTests()
            {
                var p = new JobsWithSchedulesQuery
                {
                    Id = "date.WeekdaysOfMonth.positive",
                    ScheduleDateMode = ScheduleDateMode.Weekdays,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                var n = new JobsWithSchedulesQuery
                {
                    Id = "date.WeekdaysOfMonth.negative",
                    ScheduleDateMode = ScheduleDateMode.Weekdays,
                    ScheduleTimeMode = ScheduleTimeMode.HoursMinutes,
                    ScheduleTimes = "1130"
                };

                var isWeekday = now.DayOfWeek < IsoDayOfWeek.Saturday;
                p.ScheduleDates = isWeekday ? "weekday" : "weekend";
                n.ScheduleDates = !isWeekday ? "weekday" : "weekend";

                jobs.Add(p);
                jobs.Add(n);
            }
        }
    }
}
