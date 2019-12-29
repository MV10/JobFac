namespace JobFac.Library.DataModels
{
    public enum ScheduleTimeMode
    {
        Unscheduled = 0,    // only with ScheduleDateMode.Unscheduled
        Minutes = 1,        // every hour at the indicated minutes (eg. 00,15,30,45)
        HoursMinutes = 2,   // specific times: 1130,1400,2245
        Interval = 3,       // a single value, every N minutes starting after midnight (eg. 30)
    }
}
