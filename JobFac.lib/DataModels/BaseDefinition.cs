namespace JobFac.lib.DataModels
{
    public abstract class BaseDefinition
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsStartDisabled { get; set; }
        public bool StartOnDemand { get; set; }      // ignored when IsStartDisabled is true
        public bool StartBySchedule { get; set; }    // ignored when IsStartDisabled is true

        // TODO figure out overlapping startup rules
        // query history table for instances in start/running states
        // use an enum? allow, disallow, stopotherinstance, skipthisinstance
        public bool AllowOverlappingStartup { get; set; } 

        public ScheduleDateMode ScheduleDateMode { get; set; }
        public ScheduleTimeMode ScheduleTimeMode { get; set; }
        public string ScheduleDates { get; set; }
        public string ScheduleTimes { get; set; }

        public NotificationTargetType ExecutionNotificationTargetType { get; set; }
        public NotificationTargetType SuccessNotificationTargetType { get; set; }
        public NotificationTargetType FailureNotificationTargetType { get; set; }
        public string ExecutionNotificationTarget { get; set; }
        public string SuccessNotificationTarget { get; set; }
        public string FailureNotificationTarget { get; set; }
    }
}
