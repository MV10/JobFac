namespace JobFac.Library.DataModels
{
    public enum ScheduleDateMode
    {
        Unscheduled = 0,
        DaysOfWeek = 1,         // any of 1-7 with commas, ISO format (Monday = 1, Sunday = 7)
        DaysOfMonth = 2,        // any numeric with commas, or first,last
        SpecificDates = 3,      // mm/dd,mm/dd,mm/dd
        DateRanges = 4,         // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
        WeekdaysOfMonth = 5,    // first,last
    }
}
