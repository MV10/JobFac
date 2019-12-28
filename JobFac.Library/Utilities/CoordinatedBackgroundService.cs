using System;
using System.Threading;
using System.Threading.Tasks;

// This is similar to the framework's BackgroundService, except that it returns
// a completed task from StartAsync and only calls ExecuteAsync in response to
// the ApplicationStarted event (which is really a CancellationToken, hence the
// use of Register). This way all hosted services can initialize via StartAsync
// before any of them begin doing work (assuming they all use this base class).

// Notice the ApplicationStarted.Register code in StartAsync. The lambda returns
// async void, which is generally only allowable for callbacks (aka event handlers).
// That's what Register is, so this is valid.  ExecuteAsync is not awaited, this is
// a fire-and-forget scenario. In other words, the caller, which is the token 
// Register method, doesn't care about the outcome of the Task from ExecuteAsync.

// It is therefore CRITICAL that ExecuteAsync handles *all* exceptions internally.
// There is no way for ExecuteAsync to hand off exceptions for handling higher up
// the chain, and an unhandled exception will terminate the process.

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
