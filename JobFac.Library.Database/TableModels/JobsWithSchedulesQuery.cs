using System;
using JobFac.Library.DataModels;

namespace JobFac.Library.Database
{
    public class JobsWithSchedulesQuery
    {
        public string Id { get; set; } = string.Empty;
        public ScheduleDateMode ScheduleDateMode { get; set; } = ScheduleDateMode.Unscheduled;
        public ScheduleTimeMode ScheduleTimeMode { get; set; } = ScheduleTimeMode.HoursMinutes;
        public string ScheduleDates { get; set; } = string.Empty;
        public string ScheduleTimes { get; set; } = string.Empty;
        public DateTimeKind ScheduleDateTimeKind { get; set; } = DateTimeKind.Local;
    }
}
