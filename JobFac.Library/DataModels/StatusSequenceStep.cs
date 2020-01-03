using JobFac.Library.DataModels.Abstractions;
using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class StatusSequenceStep
    {
        public int Step { get; set; } = 0;
        public int StartDecisionStepNumber { get; set; } = 0;
        public bool ExitResultSuccess { get; set; } = false;
        public bool ExitResultMixed { get; set; } = false;
        public int ExitResultStepNumber { get; set; } = 0;

        public List<JobStatus<StatusExternalProcess>> JobStatus { get; set; } = new List<JobStatus<StatusExternalProcess>>();
    }
}
