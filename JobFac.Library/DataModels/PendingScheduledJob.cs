using System;

namespace JobFac.Library.DataModels
{
    public class PendingScheduledJob
    {
        public string DefinitionId { get; set; } = string.Empty;
        public DateTimeOffset ScheduleTarget { get; set; } = DateTimeOffset.MaxValue;
    }
}
