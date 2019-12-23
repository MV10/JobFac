namespace JobFac.lib.DataModels
{
    public class StepDefinition
    {
        public string SequenceId;
        public int Step;
        public string Name;
        public string Description;

        public string JobDefinitionIdList; // comma delimited

        public StepStartDecision StartDecision1;
        public StepStartDecision StartDecision2;
        public string StartCriteria1;
        public string StartCriteria2;
        public StepAction StartTrueAction;
        public StepAction StartFalseAction;
        public int StartTrueStepNumber; // use current step# for either of these
        public int StartFalseStepNumber;

        public StepExitDecision ExitDecision;
        public StepAction ExitSuccessAction;
        public StepAction ExitFailureAction;
        public StepAction ExitMixedResultsAction;
        public int ExitSuccessStepNumber;
        public int ExitFailureStepNumber;
        public int ExitMixedStepNumber;
    }
}
