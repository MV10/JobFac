namespace JobFac.lib.DataModels
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
