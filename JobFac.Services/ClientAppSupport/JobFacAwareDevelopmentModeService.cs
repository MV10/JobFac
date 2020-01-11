using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class JobFacAwareDevelopmentModeService<TJobFacAwareProcess> : CoordinatedBackgroundService
        where TJobFacAwareProcess : JobFacAwareProcessBase
    {
        private readonly TJobFacAwareProcess process;
        private readonly JobFacAwareProcessOptions options;

        public JobFacAwareDevelopmentModeService(
            TJobFacAwareProcess targetProcess,
            IHostApplicationLifetime hostApplicationLifetime,
            JobFacAwareProcessOptions jobFacServiceOptions)
            : base(hostApplicationLifetime)
        {
            process = targetProcess;
            options = jobFacServiceOptions;
        }

        private JobFacAwareProcessContext jobFacContext;

        protected override Task InitializingAsync(CancellationToken cancelInitToken)
        {
            try
            {
                Console.WriteLine("JobFac dev-mode: Initializing");

                if (!process.ValidateArguments(options.CommandLineArgs))
                    throw new ArgumentException("Validation of the command-line arguments failed (dev-mode: no instance id)");

                if (options.RetrieveStartupPayload && !process.ValidateStartupPayload(options.DevModeStartupPayload))
                    throw new ArgumentException("Validation of the startup payload failed (dev-mode: no instance id)");

                jobFacContext = new JobFacAwareProcessContext(appLifetime, "(JobFac dev-mode)", options.CommandLineArgs, options.DevModeStartupPayload, new MockJobExternalProcess(), new MockJobServiceProvider());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JobFac dev-mode: Caught exception\n{ex}");
            }

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            try
            {
                Console.WriteLine("JobFac dev-mode: Executing job");
                await process.ExecuteProcessingAsync(jobFacContext, appStoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JobFac dev-mode: Caught exception\n{ex}");
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }
    }
}
