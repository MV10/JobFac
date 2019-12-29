using JobFac.Library.DataModels.Abstractions;
using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class StatusSequence
    {
        public int SequenceStep { get; set; } = 0;

        public List<JobStatus<StatusExternalProcess>> JobStatus { get; set; } = new List<JobStatus<StatusExternalProcess>>();
    }
}
