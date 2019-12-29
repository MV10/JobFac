namespace JobFac.Library.DataModels.Abstractions
{
    public class JobStatus<TJobTypeProperties> : JobStatusBase
        where TJobTypeProperties : class, new()
    {
        public TJobTypeProperties JobTypeProperties { get; set; } = new TJobTypeProperties();
    }
}
