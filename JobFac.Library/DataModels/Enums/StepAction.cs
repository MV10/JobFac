namespace JobFac.Library.DataModels
{
    public enum StepAction
    {
        DoNextStep = 0,
        DoStepNumber = 1,
        EndSequence = 2,
        TreatMixedAsSuccessAction = 3,
        TreatMixedAsFailureAction = 4
    }
}
