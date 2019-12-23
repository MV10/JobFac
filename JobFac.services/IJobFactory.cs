using JobFac.lib.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.services
{
    // StatelessWorker grain
    public interface IJobFactory : IGrainWithIntegerKey
    {
        Task<string> StartJob(FactoryStartOptions options);
        Task<string> StartSequence(FactoryStartOptions options);
    }
}
