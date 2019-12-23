namespace JobFac.lib.DataModels
{
    public class StepDefinition
    {
        public string SequenceId { get; set; }
        public int Step { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string JobDefinitionIdList { get; set; } // comma delimited

        public StepStartDecision StartDecision1 { get; set; }
        public StepStartDecision StartDecision2 { get; set; }
        public string StartCriteria1 { get; set; }
        public string StartCriteria2 { get; set; }
        public StepAction StartTrueAction { get; set; }
        public StepAction StartFalseAction { get; set; }
        public int StartTrueStepNumber { get; set; } // use current step# for either of these
        public int StartFalseStepNumber { get; set; }

        public StepExitDecision ExitDecision { get; set; }
        public StepAction ExitSuccessAction { get; set; }
        public StepAction ExitFailureAction { get; set; }
        public StepAction ExitMixedResultsAction { get; set; }
        public int ExitSuccessStepNumber { get; set; }
        public int ExitFailureStepNumber { get; set; }
        public int ExitMixedStepNumber { get; set; }
    }
}
