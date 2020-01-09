namespace JobFac.Services
{
    public class JobFacAwareBackgroundServiceOptions
    {
        public string[] CommandLineArgs { get; set; } = new string[0];
        public bool RetrieveStartupPayload { get; set; } = false;
        public bool FailJobOnMissingStartupPayload { get; set; } = false;
        public int ExceptionExitCode { get; set; } = -1;
    }
}
