namespace JobFac.Library.DataModels
{
    public enum RunStatus
    {
        Unknown,
        StartRequested,
        StartFailed,
        Running,
        StopRequested,
        Stopped,
        Ended,
        Failed
    }
}
