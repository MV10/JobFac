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

        public IJobExternalProcess GetExternalProcessJob(string instanceId)
            => clusterClient.GetGrain<IJobExternalProcess>(instanceId);

        public IJobFactory GetJobFactory()
            => clusterClient.GetGrain<IJobFactory>();

        public async ValueTask DisposeAsync()
        {
            await clusterClient.Close().ConfigureAwait(false);
            clusterClient.Dispose();
        }
    }
}
