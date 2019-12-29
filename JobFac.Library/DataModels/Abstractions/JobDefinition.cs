namespace JobFac.Library.DataModels.Abstractions
{
    public class JobDefinition<TJobTypeProperties> : JobDefinitionBase
        where TJobTypeProperties : class, new()
    {
        public TJobTypeProperties JobTypeProperties { get; set; } = new TJobTypeProperties();
    }
}
