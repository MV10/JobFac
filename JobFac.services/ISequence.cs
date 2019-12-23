using JobFac.lib.DataModels;
using Orleans;
using System;
using System.Threading.Tasks;

namespace JobFac.services
{
    public interface ISequence : IGrainWithStringKey
    {
        Task Start(SequenceDefinition sequenceDefinition, FactoryStartOptions options);
        Task<SequenceStatus> GetStatus();
        Task UpdateJobStatus(JobStatus jobStatus);
        Task Stop();
    }
}
