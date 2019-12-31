
namespace JobFac.Library
{
    public static class ConstLogging
    {
        // This can be used with AddFilter to apply LogLevel filtering to all JobFac log messages.
        public static readonly string JobFacLogCategoryPrefix = "JobFac.";

        // Const instead of static readonly because it's used in an attribute (JobFacLoggerProvider)
        // which requires constants; static readonly is preferable because a constant's value is
        // directly replaced in the IL rather than a reference to the originating assembly field. This
        // is also used as the log message category when the remote message is repeated to the Silo
        // host logger, which allows the host to reference this const to filter by category (or the
        // more general JobFac prefix).
        public const string JobFacRemoteLoggerProviderName = "JobFac.RemoteLogging";

        public static readonly string JobFacCategorySchedulerService = JobFacLogCategoryPrefix + "SchedulerService";
        public static readonly string JobFacCategorySchedulerQueue = JobFacLogCategoryPrefix + "SchedulerQueue";
        public static readonly string JobFacCategoryScheduleWriter = JobFacLogCategoryPrefix + "ScheduleWriter";

        public static readonly string LogEntryTimestampFormat = "yyyy-MM-dd HH:mm:ss.fff UTC";
        public static readonly int DefaultBackgroundQueueSize = 1000;
        public static readonly int DefaultFlushQueueSeconds = 5;
    }
}
