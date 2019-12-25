using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class FactoryStartOptions
    {
        // job or sequence, based on the JobFactory Start method used
        public string DefinitionId { get; set; } = string.Empty;

        // this is in addition to notifications in the underlying definition
        public NotificationScope NotificationScope { get; set; } = NotificationScope.None;
        public NotificationOrigin NotificationOrigin { get; set; } = NotificationOrigin.Job;
        public NotificationTargetType NotificationTargetType { get; set; } = NotificationTargetType.None;
        public string NotificationTarget { get; set; } = string.Empty;

        // key is job definition Id
        public Dictionary<string, string> ReplacementArguments { get; set; } = new Dictionary<string, string>();    // replaces args in a job definition
        public Dictionary<string, string> StartupPayloads { get; set; } = new Dictionary<string, string>();         // for JobFac-aware apps (PrefixJobInstanceIdArgument)
    }
}
