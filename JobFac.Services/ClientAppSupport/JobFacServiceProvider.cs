using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class JobFacServiceProvider : IJobFacServiceProvider
    {
        private readonly IClusterClient clusterClient;

        public JobFacServiceProvider(IClusterClient clusterClient)
        {
            this.clusterClient = clusterClient;
        }

        public IJob GetJob(string jobInstanceId)
            => clusterClient.GetGrain<IJob>(jobInstanceId);

        public IJobFactory GetJobFactory()
            => clusterClient.GetGrain<IJobFactory>(0);

        public ISequence GetSequence(string sequenceInstanceId)
            => clusterClient.GetGrain<ISequence>(sequenceInstanceId);

        public async ValueTask DisposeAsync()
        {
            await clusterClient.Close();
            clusterClient.Dispose();
        }
    }
}
