using System;
using System.Collections.Generic;

namespace JobFac.lib.DataModels
{
    public class SequenceStatus
    {
        public string Key;
        public FactoryStartOptions StartOptions;
        public DateTimeOffset StateDataPurgeTarget;

        public int SequenceStep;

        public DateTimeOffset LastUpdated;
        public RunStatus RunStatus;
        public DateTimeOffset StartRequested;
        public DateTimeOffset ExitStateReceived;
        public List<JobStatus> JobStatus;
    }
}
