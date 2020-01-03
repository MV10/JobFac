using JobFac.Library.DataModels.Abstractions;
using System.Collections.Generic;

namespace JobFac.Library.DataModels
{
    public class StatusSequence
    {
        public int SequenceStep { get; set; } = 0;

        public Dictionary<int, StatusSequenceStep> StepStatus { get; set; } = new Dictionary<int, StatusSequenceStep>();
    }
}
