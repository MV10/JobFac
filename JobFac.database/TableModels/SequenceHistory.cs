using JobFac.lib.DataModels;
using System;

namespace JobFac.database
{
    public class SequenceHistory
    {
        public string InstanceKey { get; set; }
        public string DefinitionId { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset DeleteAfter { get; set; }
        public RunStatus FinalRunStatus { get; set; }
        public string FullDetailsJson { get; set; }
    }
}
