namespace JobFac.Library.DataModels
{
    public enum StepStartDecision
    {
        NoDecision = 0,
        IfDate = 1,     // mm/dd
        IfBeforeDate = 2,
        IfAfterDate = 3,
        IfDateRange = 4,
        IfMonth = 5,
        IfDayOfMonth = 6,
        IfDayOfWeek = 7,
        IfTime = 8,     // hh:mm
        IfBeforeTime = 9,
        IfAfterTime = 10,
        IfTimeRange = 11
    }
}
