namespace JobFac.Library.DataModels
{
    public enum StepExitDecision
    {
        DoActionWhenAllExit = 0,
        DoActionWhenAnyExit = 1,
        DoNextStepWithoutWaiting = 2
    }
}
