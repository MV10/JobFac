using JobFac.lib.DataModels;
using System;

namespace JobFac.database
{
    public class SequenceHistory
    {
        public string InstanceKey;
        public string DefinitionId;
        public DateTimeOffset LastUpdated;
        public DateTimeOffset DeleteAfter;
        public RunStatus FinalRunStatus;
        public string FullDetailsJson;
    }
}
