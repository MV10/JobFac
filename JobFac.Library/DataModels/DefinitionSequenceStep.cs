namespace JobFac.Library.DataModels
{
    public class DefinitionSequenceStep
    {
        public string SequenceId { get; set; } = string.Empty;
        public int Step { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string JobDefinitionIdList { get; set; } = string.Empty; // comma delimited

        public StepStartDateDecision StartDateDecision { get; set; } = StepStartDateDecision.NoDecision;
        public StepStartTimeDecision StartTimeDecision { get; set; } = StepStartTimeDecision.NoDecision;
        public string StartDates { get; set; } = string.Empty;
        public string StartTimes { get; set; } = string.Empty;
        public string StartDecisionTimeZone { get; set; } = "America/New_York"; // UTC is "Etc/UTC" https://nodatime.org/TimeZones
        public StepAction StartFalseAction { get; set; } = StepAction.DoStepNumber;
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
