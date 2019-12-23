namespace JobFac.lib.DataModels
{
    public class JobDefinition : BaseDefinition
    {
        public string ExecutablePathname;
        public string WorkingDirectory;

        public string Username; // UPN format name@foo.bar.com
        public string Password;

        public bool StartInSequence;    // ignored when IsStartDisabled is true (BaseDefinition)

        public string Arguments;
        public bool AllowReplacementArguments;
        public bool PrefixJobInstanceIdArgument;

        public bool LogStdOut;          // writes to logger
        public bool LogStdErr;
        public bool BulkUpdateStdOut;   // writes to DB
        public bool BulkUpdateStdErr;

        public bool RequireMinimumRunTime;
        public int MinimumRunSeconds;
        public NotificationTargetType MinimumRunTimeNotificationTargetType;
        public string MinimumRunTimeNotificationTarget;

        public bool ObserveMaximumRunTime;
        public int MaximumRunSeconds;
        public bool StopLongRunningJob;
        public NotificationTargetType MaximumRunTimeNotificationTargetType;
        public string MaximumRunTimeNotificationTarget;

        public bool RetryWhenFailed;
        public bool OnlyNotifyOnce;
        public bool AllowRetryInSequences;
        public int MaximumRetryCount;
        public int RetryDelaySeconds;
    }
}
