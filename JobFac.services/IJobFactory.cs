using JobFac.Library.DataModels;
using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services
{
    // StatelessWorker grain
    public interface IJobFactory : IGrainWithIntegerKey
    {
        Task<string> StartJob(FactoryStartOptions options);
        Task<string> StartJob(FactoryStartOptions options, string replacementArguments = null, string startupPayload = null);
        Task<string> StartSequence(FactoryStartOptions options);
        Task<IReadOnlyList<string>> GetRunningJobInstanceIds(string definitionId);
        Task<IReadOnlyList<string>> GetRunningSequenceInstanceIds(string definitionId);
    }
}
