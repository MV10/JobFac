using JobFac.Library.DataModels.Abstractions;
using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class StatusSequenceStep
    {
        public int Step { get; set; } = 0;
        public bool StartDecisionSuccess { get; set; } = true;

        // ignored if ExitDecision == DoNextStepWithoutWaiting
        public bool ExitResultSuccess { get; set; } = false;
        public bool ExitResultMixed { get; set; } = false;
        public int ExitResultStepNumber { get; set; } = 0;

        // keyed on job instance ID
        public Dictionary<string, JobStatus<StatusExternalProcess>> JobStatus { get; set; } = new Dictionary<string, JobStatus<StatusExternalProcess>>();
    }
}
