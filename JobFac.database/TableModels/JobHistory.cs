using JobFac.lib.DataModels;
using System;

namespace JobFac.database
{
    public class JobHistory
    {
        public string InstanceKey { get; set; } = string.Empty;
        public string DefinitionId { get; set; } = string.Empty;
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
        public DateTimeOffset DeleteAfter { get; set; } = DateTimeOffset.MaxValue;
        public RunStatus FinalRunStatus { get; set; } = RunStatus.Unknown;
        public int ExitCode { get; set; } = 0;
        public string FullDetailsJson { get; set; } = string.Empty;
    }
}
