using System;

namespace JobFac.Library.DataModels
{
    public class JobStatus
    {
        public string Key { get; set; } = string.Empty;
        public FactoryStartOptions StartOptions { get; set; } = null;

        public string SequenceDefinitionId { get; set; } = string.Empty;
        public string SequenceKey { get; set; } = string.Empty;
        public int SequenceStep { get; set; } = 0;

        public string MachineName { get; set; } = string.Empty;
        public RunStatus RunStatus { get; set; } = RunStatus.Unknown;
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset StartRequested { get; set; } = DateTime.MinValue;
        public DateTimeOffset ExitStateReceived { get; set; } = DateTimeOffset.MinValue;

        public bool HasStarted { get; set; } = false;
        public bool HasExited { get; set; } = false;
        public bool HasFailed { get; set; } = false;

        public int ExitCode { get; set; } = 0;
        public string ExitMessage { get; set; } = string.Empty;
    }
}
