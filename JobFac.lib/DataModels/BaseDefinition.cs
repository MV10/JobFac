namespace JobFac.lib.DataModels
{
    public abstract class BaseDefinition
    {
        public string Id;
        public string Category;
        public string Name;
        public string Description;

        public bool IsStartDisabled;
        public bool StartOnDemand;      // ignored when IsStartDisabled is true
        public bool StartBySchedule;    // ignored when IsStartDisabled is true

        // TODO figure out overlapping startup rules
        // query history table for instances in start/running states
        // use an enum? allow, disallow, stopotherinstance, skipthisinstance
        public bool AllowOverlappingStartup; 

        public ScheduleDateMode ScheduleDateMode;
        public ScheduleTimeMode ScheduleTimeMode;
        public string ScheduleDates;
        public string ScheduleTimes;

        public NotificationTargetType ExecutionNotificationTargetType;
        public NotificationTargetType SuccessNotificationTargetType;
        public NotificationTargetType FailureNotificationTargetType;
        public string ExecutionNotificationTarget;
        public string SuccessNotificationTarget;
        public string FailureNotificationTarget;
    }
}
