using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class StatusSequence
    {
        public int SequenceStep { get; set; } = 0;

        // keyed on step number
        public Dictionary<int, StatusSequenceStep> StepStatus { get; set; } = new Dictionary<int, StatusSequenceStep>();

        // key is spawned job instance id, value is step number (used in SequencedJobStatusChanged call)
        public Dictionary<string, int> JobInstanceStepMap { get; set; } = new Dictionary<string, int>();
    }
}
