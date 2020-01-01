using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Services.Scheduling;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
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

        protected override async Task ExecuteAsync()
        {
            try
            {
                var jobs = GetTestJobs();
                var now = SystemClock.Instance.GetCurrentInstant();
                var target = now.Plus(Duration.FromDays(1));

                Console.WriteLine($"In TZ:  {tz.Id}");
                Console.WriteLine($"Now:    {ZonedDateTimePattern.GeneralFormatOnlyIso.Format(now.InZone(tz))}");
                Console.WriteLine($"Target: {ZonedDateTimePattern.GeneralFormatOnlyIso.Format(target.InZone(tz))}\n");

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
            Console.WriteLine($"Job:\n  Date Mode: {job.ScheduleDateMode} = {job.ScheduleDates}\n  Time Mode: {job.ScheduleTimeMode} = {job.ScheduleTimes}\n  Time Zone: {job.ScheduleTimeZone}");
            var planner = new TargetPlanner(job, targetDate);
            var results = planner.GetSchedules();
            Console.WriteLine($"  Schedules: {results.Count}");
            foreach(var s in results)
            {
                var zoned = ZonedDateTime.FromDateTimeOffset(s.ScheduleTarget).WithZone(tz);
                Console.WriteLine($"  {ZonedDateTimePattern.ExtendedFormatOnlyIso.Format(zoned)}");
            }
        }

        private List<JobsWithSchedulesQuery> GetTestJobs()
            => new List<JobsWithSchedulesQuery>
            {
                new JobsWithSchedulesQuery
                {
                    Id = "x",
                    ScheduleDateMode = ScheduleDateMode.DaysOfWeek,
                    ScheduleDates = "1,2,3,4,5,6,7",
                    ScheduleTimeMode = ScheduleTimeMode.Interval,
                    ScheduleTimes = "10"
                },
            };
    }
}
