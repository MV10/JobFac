using System;

namespace JobFac.lib.DataModels
{
    public class JobStatus
    {
        public string Key;
        public FactoryStartOptions StartOptions;
        public DateTimeOffset StateDataPurgeTarget;

        public string SequenceDefinitionId;
        public string SequenceKey;
        public int SequenceStep;

        public RunStatus RunStatus;
        public DateTimeOffset LastUpdated;
        public DateTimeOffset StartRequested;
        public DateTimeOffset ExitStateReceived;

        public bool HasStarted;
        public bool HasExited;
        public bool HasFailed;

        public int ExitCode;
        public string ExitMessage;
    }
}
