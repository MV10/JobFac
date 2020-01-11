namespace JobFac.Services
{
    public class JobFacAwareProcessOptions
    {
        public string[] CommandLineArgs { get; set; } = new string[0];
        public bool RetrieveStartupPayload { get; set; } = false;
        public bool FailJobOnMissingStartupPayload { get; set; } = false;
        public int ExceptionExitCode { get; set; } = -1;

        internal string DevModeStartupPayload { get; set; } = string.Empty;
    }
}
