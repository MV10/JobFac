using System.Collections.Generic;

namespace JobFac.lib.DataModels
{
    public class FactoryStartOptions
    {
        // job or sequence, based on the JobFactory Start method used
        public string DefinitionId { get; set; }

        // this is in addition to notifications in the underlying definition
        public NotificationScope NotificationScope { get; set; }
        public NotificationOrigin NotificationOrigin { get; set; }
        public NotificationTargetType NotificationTargetType { get; set; }
        public string NotificationTarget { get; set; }

        // key is job definition Id
        public Dictionary<string, string> ReplacementArguments { get; set; } // replaces args in a job definition
        public Dictionary<string, string> StartupPayloads { get; set; }      // for JobFac-aware apps (PrefixJobInstanceIdArgument)
    }
}
