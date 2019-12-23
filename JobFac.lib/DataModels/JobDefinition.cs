namespace JobFac.lib.DataModels
{
    public class JobDefinition : BaseDefinition
    {
        public string ExecutablePathname { get; set; }
        public string WorkingDirectory { get; set; }

        public string Username { get; set; } // UPN format name@foo.bar.com
        public string Password { get; set; }

        public bool StartInSequence { get; set; }    // ignored when IsStartDisabled is true (BaseDefinition)

        public string Arguments { get; set; }
        public bool AllowReplacementArguments { get; set; }
        public bool PrefixJobInstanceIdArgument { get; set; }

        public bool LogStdOut { get; set; }          // writes to logger
        public bool LogStdErr { get; set; }
        public bool BulkUpdateStdOut { get; set; }   // writes to DB
        public bool BulkUpdateStdErr { get; set; }

        public bool RequireMinimumRunTime { get; set; }
        public int MinimumRunSeconds { get; set; }
        public NotificationTargetType MinimumRunTimeNotificationTargetType { get; set; }
        public string MinimumRunTimeNotificationTarget { get; set; }

        public bool ObserveMaximumRunTime { get; set; }
        public int MaximumRunSeconds { get; set; }
        public bool StopLongRunningJob { get; set; }
        public NotificationTargetType MaximumRunTimeNotificationTargetType { get; set; }
        public string MaximumRunTimeNotificationTarget { get; set; }

        public bool RetryWhenFailed { get; set; }
        public bool OnlyNotifyOnce { get; set; }
        public bool AllowRetryInSequences { get; set; }
        public int MaximumRetryCount { get; set; }
        public int RetryDelaySeconds { get; set; }
    }
}
