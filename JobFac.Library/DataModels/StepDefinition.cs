namespace JobFac.Library.DataModels
{
    public class StepDefinition
    {
        public string SequenceId { get; set; } = string.Empty;
        public int Step { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string JobDefinitionIdList { get; set; } = string.Empty; // comma delimited

        public StepStartDecision StartDecision1 { get; set; } = StepStartDecision.NoDecision;
        public StepStartDecision StartDecision2 { get; set; } = StepStartDecision.NoDecision;
        public string StartCriteria1 { get; set; } = string.Empty;
        public string StartCriteria2 { get; set; } = string.Empty;
        public StepAction StartTrueAction { get; set; } = StepAction.DoStepNumber;
        public StepAction StartFalseAction { get; set; } = StepAction.DoStepNumber;
        public int StartTrueStepNumber { get; set; } = 0;   // use current step# for either of these
        public int StartFalseStepNumber { get; set; } = 0;

        public StepExitDecision ExitDecision { get; set; } = StepExitDecision.DoActionWhenAllExit;
        public StepAction ExitSuccessAction { get; set; } = StepAction.DoNextStep;
        public StepAction ExitFailureAction { get; set; } = StepAction.DoNextStep;
        public StepAction ExitMixedResultsAction { get; set; } = StepAction.DoNextStep;
        public int ExitSuccessStepNumber { get; set; } = 0;
        public int ExitFailureStepNumber { get; set; } = 0;
        public int ExitMixedStepNumber { get; set; } = 0;
    }
}
