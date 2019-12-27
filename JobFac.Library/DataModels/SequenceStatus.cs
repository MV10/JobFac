using System;
using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class SequenceStatus
    {
        public string Key { get; set; } = string.Empty;
        public FactoryStartOptions StartOptions { get; set; } = null;

        public int SequenceStep { get; set; } = 0;
        public bool HasStarted { get; set; } = false;
        public bool HasExited { get; set; } = false;
        public bool HasFailed { get; set; } = false;

        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
        public RunStatus RunStatus { get; set; } = RunStatus.Unknown;
        public DateTimeOffset StartRequested { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset ExitStateReceived { get; set; } = DateTimeOffset.MinValue;
        public List<JobStatus> JobStatus { get; set; } = new List<JobStatus>();
    }
}
