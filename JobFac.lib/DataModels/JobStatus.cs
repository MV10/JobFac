using System;

namespace JobFac.lib.DataModels
{
    public class JobStatus
    {
        public string Key { get; set; }
        public FactoryStartOptions StartOptions { get; set; }
        public DateTimeOffset StateDataPurgeTarget { get; set; }

        public string SequenceDefinitionId { get; set; }
        public string SequenceKey { get; set; }
        public int SequenceStep { get; set; }

        public RunStatus RunStatus { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset StartRequested { get; set; }
        public DateTimeOffset ExitStateReceived { get; set; }

        public bool HasStarted { get; set; }
        public bool HasExited { get; set; }
        public bool HasFailed { get; set; }

        public int ExitCode { get; set; }
        public string ExitMessage { get; set; }
    }
}
