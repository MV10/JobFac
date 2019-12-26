using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class ClusterClientHostedService : IHostedService
    {
        private readonly IJobFacServiceProvider orleansClusterClientWrapper;

        public ClusterClientHostedService(IJobFacServiceProvider clientWrapper)
            => orleansClusterClientWrapper = clientWrapper;

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
            => await orleansClusterClientWrapper.DisposeAsync();
    }
}
