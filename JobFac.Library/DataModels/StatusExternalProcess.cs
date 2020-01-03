namespace JobFac.Library.DataModels
{
    public class StatusExternalProcess
    {
        public string MachineName { get; set; } = string.Empty;

        public int ExitCode { get; set; } = 0;
        public string ExitMessage { get; set; } = string.Empty;
    }
}
