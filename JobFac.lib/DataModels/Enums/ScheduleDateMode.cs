namespace JobFac.lib.DataModels
{
    public enum ScheduleDateMode
    {
        None,
        DaysOfWeek,         // 0-6 with commas
        FirstLastWeekday,   // first/last
        DaysOfMonth,        // numeric with commas, or first/last
        SpecificDate,       // mm/dd
        DateRangeInclusive, // mm/dd, mm/dd
        DateRangeExclusive, // mm/dd, mm/dd
    }
}
