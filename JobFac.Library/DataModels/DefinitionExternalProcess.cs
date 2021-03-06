﻿namespace JobFac.Library.DataModels
{
    public class DefinitionExternalProcess
    {
        public string ExecutablePathname { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty; // UPN format name@foo.bar.com
        public string Password { get; set; } = string.Empty;

        public bool StartInSequence { get; set; } = true;    // ignored when IsStartDisabled is true (BaseDefinition)

        public string Arguments { get; set; } = string.Empty;
        public bool AllowReplacementArguments { get; set; } = true;

        // true implies:
        // first command-line argument to job is the instance key
        // can call IJob service to pick up more detailed payload
        // must call IJob.UpdateExitMessage prior to exiting
        public bool IsJobFacAware { get; set; } = false;

        public JobStreamHandling CaptureStdOut { get; set; } = JobStreamHandling.None;
        public JobStreamHandling CaptureStdErr { get; set; } = JobStreamHandling.None;
        public string StdOutPathname { get; set; } = string.Empty; // put * in the filename if timestamp-replacement is specified
        public string StdErrPathname { get; set; } = string.Empty;

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
