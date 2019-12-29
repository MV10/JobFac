namespace JobFac.Library.DataModels
{
    public enum ScheduleTimeMode
    {
        Unscheduled = 0,    // only with ScheduleDateMode.Unscheduled
        Minutes = 1,        // every hour
        HoursMinutes = 2    // specific times: 1130,1400,2245
    }
}
