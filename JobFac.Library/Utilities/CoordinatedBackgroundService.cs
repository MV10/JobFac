using System;
using System.Threading;
using System.Threading.Tasks;

// This is similar to the framework's BackgroundService, except that it returns
// a completed task from StartAsync and only calls ExecuteAsync in response to
// the ApplicationStarted event (which is really a CancellationToken, hence the
// use of Register). This way all hosted services can initialize via StartAsync
// before any of them begin doing work (assuming they all use this base class).

namespace Microsoft.Extensions.Hosting
{
    public abstract class CoordinatedBackgroundService : IHostedService, IDisposable
    {
        protected readonly IHostApplicationLifetime appLifetime;

        public CoordinatedBackgroundService(IHostApplicationLifetime appLifetime)
        {
            this.appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // since this is an event handler, the lambda's async void is acceptable
            appLifetime.ApplicationStarted.Register(async () => await ExecuteAsync());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        protected abstract Task ExecuteAsync();

        public virtual void Dispose()
        { }
    }
}
