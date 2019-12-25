using JobFac.Library.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    // StatelessWorker grain
    public interface IJobFactory : IGrainWithIntegerKey
    {
        Task<string> StartJob(FactoryStartOptions options);
        Task<string> StartSequence(FactoryStartOptions options);
    }
}
