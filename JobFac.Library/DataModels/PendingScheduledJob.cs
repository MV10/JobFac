namespace JobFac.Library.DataModels
{
    public class PendingScheduledJob
    {
        public string DefinitionId { get; set; } = string.Empty;
        public long ScheduleTarget { get; set; }
    }
}
