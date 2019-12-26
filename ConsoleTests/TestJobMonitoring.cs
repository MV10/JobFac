using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestJobMonitoring : IHostedService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IJobFacServiceProvider jobFacServices;

        public TestJobMonitoring(
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices)
        {
            this.appLifetime = appLifetime;
            this.jobFacServices = jobFacServices;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // since this is an event handler, the lambda's async void is acceptable
            appLifetime.ApplicationStarted.Register(async () => await ExecuteAsync());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("Getting job factory proxy.");
                var factory = jobFacServices.GetJobFactory();

                var options = new FactoryStartOptions
                {
                    DefinitionId = "Sample.JobFac.unaware"
                };

                Console.WriteLine("Starting sample job: Sample.JobFac.unaware");
                var jobKey = await factory.StartJob(options);
                Console.WriteLine($"Job instance key: {jobKey}");

                var timeout = DateTimeOffset.UtcNow.AddSeconds(90);
                bool done = false;
                IJob job = null;
                while (!done && DateTimeOffset.UtcNow < timeout)
                {
                    Console.WriteLine("Pausing 10 seconds then reading status.");
                    await Task.Delay(10000);

                    if (job == null) job = jobFacServices.GetJob(jobKey);
                    if (job == null)
                    {
                        Console.WriteLine("Failed to obtain job proxy.");
                        done = true;
                        break;
                    }

                    var status = await job.GetStatus();
                    Console.WriteLine($"Status {status.RunStatus} last updated {status.LastUpdated.ToLocalTime()}");
                    done = status.HasExited;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n\nException:\n{ex}");
            }

            appLifetime.StopApplication();
        }
    }
}
