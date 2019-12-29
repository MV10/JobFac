namespace JobFac.Library.DataModels
{
    public enum ScheduleDateMode
    {
        Unscheduled = 0,
        DaysOfWeek = 1,         // 0-6 with commas
        FirstLastWeekday = 2,   // first/last
        DaysOfMonth = 3,        // numeric with commas, or first/last
        SpecificDate = 4,       // mm/dd
        DateRangeInclusive = 5, // mm/dd, mm/dd
        DateRangeExclusive = 6, // mm/dd, mm/dd
    }
}
