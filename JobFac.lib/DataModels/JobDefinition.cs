namespace JobFac.lib.DataModels
{
    public class JobDefinition : BaseDefinition
    {
        public string ExecutablePathname { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty; // UPN format name@foo.bar.com
        public string Password { get; set; } = string.Empty;

        public bool StartInSequence { get; set; } = true;    // ignored when IsStartDisabled is true (BaseDefinition)

        public string Arguments { get; set; } = string.Empty;
        public bool AllowReplacementArguments { get; set; } = true;
        public bool PrefixJobInstanceIdArgument { get; set; } = false;

        public bool LogStdOut { get; set; } = false;        // writes to logger
        public bool LogStdErr { get; set; } = false;
        public bool CaptureStdOut { get; set; } = false;    // writes to DB
        public bool CaptureStdErr { get; set; } = false;

        // TODO add stdout and stderr paths

        public bool RequireMinimumRunTime { get; set; } = false;
        public int MinimumRunSeconds { get; set; } = 0;
        public NotificationTargetType MinimumRunTimeNotificationTargetType { get; set; } = NotificationTargetType.None;
        public string MinimumRunTimeNotificationTarget { get; set; } = string.Empty;

        public bool ObserveMaximumRunTime { get; set; } = false;
        public int MaximumRunSeconds { get; set; } = 0;
        public bool StopLongRunningJob { get; set; } = false;
        public NotificationTargetType MaximumRunTimeNotificationTargetType { get; set; } = NotificationTargetType.None;
        public string MaximumRunTimeNotificationTarget { get; set; } = string.Empty;

        public bool RetryWhenFailed { get; set; } = false;
        public bool OnlyNotifyOnce { get; set; } = true;
        public bool AllowRetryInSequences { get; set; } = true;
        public int MaximumRetryCount { get; set; } = 1;
        public int RetryDelaySeconds { get; set; } = 0;
    }
}
