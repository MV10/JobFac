using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public interface IJobSequence : IGrainWithStringKey
    {
        Task Start(JobDefinition<DefinitionSequence> jobDefinition, FactoryStartOptions options);
        Task<JobStatus<StatusSequence>> GetStatus();
        Task SequencedJobStatusChanged(JobStatus<StatusExternalProcess> jobStatus);
        Task EndSequence();
        Task Stop();
    }
}
