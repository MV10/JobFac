using System;
using System.Collections.Generic;

namespace JobFac.lib.DataModels
{
    public class SequenceStatus
    {
        public string Key { get; set; }
        public FactoryStartOptions StartOptions { get; set; }
        public DateTimeOffset StateDataPurgeTarget { get; set; }

        public int SequenceStep { get; set; }

        public DateTimeOffset LastUpdated { get; set; }
        public RunStatus RunStatus { get; set; }
        public DateTimeOffset StartRequested { get; set; }
        public DateTimeOffset ExitStateReceived { get; set; }
        public List<JobStatus> JobStatus { get; set; }
    }
}
