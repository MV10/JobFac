using System;

namespace JobFac.Library.DataModels.Abstractions
{
    public class JobStatusBase
    {
        public string Key { get; set; } = string.Empty;
        public FactoryStartOptions StartOptions { get; set; } = null;

        public RunStatus RunStatus { get; set; } = RunStatus.Unknown;
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset StartRequested { get; set; } = DateTime.MinValue;
        public DateTimeOffset ExitStateReceived { get; set; } = DateTimeOffset.MinValue;

        public bool HasStarted { get; set; } = false;
        public bool HasExited { get; set; } = false;
        public bool HasFailed { get; set; } = false;
    }
}
