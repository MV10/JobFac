namespace JobFac.Library.Database
{
    public class ScheduledJobs
    {
        public string DefinitionId { get; set; } = string.Empty;
        public long ScheduleTarget { get; set; } = 0; // yyyyMMddHHmm ... noon on xmas 2020: 202012251200
        public string Activation { get; set; } = string.Empty;
    }
}
