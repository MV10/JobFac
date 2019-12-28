namespace JobFac.Library.DataModels
{
    public enum NotificationTargetType
    {
        None = 0,
        UriTrigger = 1,     // no data passed
        UriPostStatus = 2,  // posts data
        Email = 3,          // internally generated message
    }
}
