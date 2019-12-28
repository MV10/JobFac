namespace JobFac.Library.DataModels
{
    public enum RunStatus
    {
        Unknown = 0,
        StartRequested = 1,
        StartFailed = 2,
        Running = 3,
        StopRequested = 4,
        Stopped = 5,
        Ended = 6,
        Failed =7
    }
}
