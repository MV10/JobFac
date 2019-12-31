
namespace JobFac.Library
{
    public static class ConstConfigKeys
    {
        // APPSETTINGS.JSON KEYS //////////////////////////////////////////////////////////////////

        public static readonly string ServicesConnectionStringName = "JobFacServices";
        public static readonly string StorageConnectionStringName = "JobFacStorage";

        public static readonly string HistoryRetentionDays = "HistoryRetentionDays"; // TODO move to config table
        public static readonly string RunnerExecutablePathname = "RunnerExecutablePathname";

        // DATABASE CONFIG-TABLE KEYS /////////////////////////////////////////////////////////////

        public static readonly string ScheduleWriterLastRunDateUtc = "ScheduleWriterLastRunDateUtc";
        public static readonly string ScheduleWriterRunTargetUtc = "ScheduleWriterRunTargetUtc";
        public static readonly string SchedulerQueueMaxJobAssignment = "SchedulerQueueMaxJobAssignment";
    }
}
