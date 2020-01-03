namespace JobFac.Library.DataModels
{
    public enum StepStartDateDecision
    {
        NoDecision = 0,
        DaysOfWeek = 1,         // any of 1-7 with commas, ISO format (Monday = 1, Sunday = 7)
        DaysOfMonth = 2,        // any numeric with commas, or first,last
        SpecificDates = 3,      // mm/dd,mm/dd,mm/dd
        DateRanges = 4,         // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
        Weekdays = 5,           // first,last,weekday,weekend
    }
}
