﻿namespace JobFac.Library.DataModels
{
    public enum NotificationTargetType
    {
        None,
        UriTrigger,     // no data passed
        UriPostStatus,  // posts data
        Email,          // internally generated message
    }
}