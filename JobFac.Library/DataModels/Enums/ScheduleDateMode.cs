namespace JobFac.Library.DataModels
{
    public enum ScheduleDateMode
    {
        Unscheduled = 0,
        DaysOfWeek = 1,         // any of 0-6 with commas
        DaysOfMonth = 2,        // any numeric with commas, or first,last
        SpecificDates = 3,      // mm/dd,mm/dd,mm/dd
        DateRanges = 4,         // mm/dd-mm/dd,mm/dd-mm/dd (inclusive)
        WeekdaysOfMonth = 5,    // first,last
    }
}
