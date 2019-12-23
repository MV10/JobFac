using JobFac.lib.DataModels;
using System;

namespace JobFac.database
{
    public class JobHistory
    {
        public string InstanceKey { get; set; }
        public string DefinitionId { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset DeleteAfter { get; set; }
        public RunStatus FinalRunStatus { get; set; }
        public int ExitCode { get; set; }
        public string FullDetailsJson { get; set; }
    }
}
