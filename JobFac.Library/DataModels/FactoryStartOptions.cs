using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class FactoryStartOptions
    {
        public string DefinitionId { get; set; } = string.Empty;

        // An ad-hoc notification in addition to notifications in the underlying definitions.
        public NotificationScope NotificationScope { get; set; } = NotificationScope.None;
        public NotificationOrigin NotificationOrigin { get; set; } = NotificationOrigin.Job;
        public NotificationTargetType NotificationTargetType { get; set; } = NotificationTargetType.None;
        public string NotificationTarget { get; set; } = string.Empty;

        // Optional, key is ExternalProcess Id. ReplacementArguments overrides any arguments provided
        // by the job definition. StartupPayloads are retrieved on-demand by JobFac-aware processes.
        public Dictionary<string, string> ReplacementArguments { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> StartupPayloads { get; set; } = new Dictionary<string, string>();

        // Populated by JobSequence service when starting an ExternalProcess as
        // part of a Sequence so the ExternalProcess can report status changes.
        public string SequenceInstanceId { get; set; } = string.Empty;
    }
}
