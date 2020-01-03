namespace JobFac.Library.DataModels
{
    public enum StepStartTimeDecision
    {
        NoDecision = 0,
        IfHours = 1,        // HH,HH,HH
        IfMinutes = 2,      // mm,mm,mm
        IfTime = 3,         // HHmm,HHmm
        IfTimeRange = 4     // HHmm-HHmm,HHmm-HHmm
    }
}
