namespace JobFac.Library.DataModels
{
    public enum StepStartDecision
    {
        NoDecision,
        IfDate,             // mm/dd
        IfBeforeDate,
        IfAfterDate,
        IfDateRange,
        IfMonth,
        IfDayOfMonth,
        IfDayOfWeek,
        IfTime,             // hh:mm
        IfBeforeTime,
        IfAfterTime,
        IfTimeRange
    }
}
