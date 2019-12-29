namespace JobFac.Library.DataModels
{
    public class StatusExternalProcess
    {
        public string MachineName { get; set; } = string.Empty;

        public string SequenceDefinitionId { get; set; } = string.Empty;
        public string SequenceKey { get; set; } = string.Empty;
        public int SequenceStep { get; set; } = 0;

        public int ExitCode { get; set; } = 0;
        public string ExitMessage { get; set; } = string.Empty;
    }
}
