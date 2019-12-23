using System.Collections.Generic;

namespace JobFac.lib.DataModels
{
    public class FactoryStartOptions
    {
        // job or sequence, based on the JobFactory Start method used
        public string DefinitionId;

        // this is in addition to notifications in the underlying definition
        public NotificationScope NotificationScope;
        public NotificationOrigin NotificationOrigin;
        public NotificationTargetType NotificationTargetType;
        public string NotificationTarget;

        // key is job definition Id
        public Dictionary<string, string> ReplacementArguments; // replaces args in a job definition
        public Dictionary<string, string> StartupPayloads;      // for JobFac-aware apps (PrefixJobInstanceIdArgument)
    }
}
