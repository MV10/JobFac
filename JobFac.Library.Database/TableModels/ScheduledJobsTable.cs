using System;

namespace JobFac.Library.Database
{
    public class ScheduledJobsTable
    {
        public string DefinitionId { get; set; } = string.Empty;
        public DateTimeOffset ScheduleTarget { get; set; } = DateTimeOffset.MaxValue; // stored as UTC, convert to Noda Time Instant
        public string Activation { get; set; } = string.Empty;
    }
}
