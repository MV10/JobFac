﻿namespace JobFac.Library.DataModels
{
    public abstract class BaseDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public bool IsStartDisabled { get; set; } = false;
        public bool StartOnDemand { get; set; } = false;    // ignored when IsStartDisabled is true
        public bool StartBySchedule { get; set; } = false;  // ignored when IsStartDisabled is true

        public AlreadyRunningAction AlreadyRunningAction { get; set; } = AlreadyRunningAction.StartNormally;
        public NotificationTargetType AlreadyRunningNotificationTargetType { get; set; } = NotificationTargetType.None;
        public string AlreadyRunningNotificationTarget { get; set; } = string.Empty;

        public ScheduleDateMode ScheduleDateMode { get; set; } = ScheduleDateMode.None;
        public ScheduleTimeMode ScheduleTimeMode { get; set; } = ScheduleTimeMode.None;
        public string ScheduleDates { get; set; } = string.Empty;
        public string ScheduleTimes { get; set; } = string.Empty;

        public NotificationTargetType ExecutionNotificationTargetType { get; set; } = NotificationTargetType.None;
        public NotificationTargetType SuccessNotificationTargetType { get; set; } = NotificationTargetType.None;
        public NotificationTargetType FailureNotificationTargetType { get; set; } = NotificationTargetType.None;
        public string ExecutionNotificationTarget { get; set; } = string.Empty;
        public string SuccessNotificationTarget { get; set; } = string.Empty;
        public string FailureNotificationTarget { get; set; } = string.Empty;
    }
}
