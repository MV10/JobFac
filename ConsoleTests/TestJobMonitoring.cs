using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestJobMonitoring : CoordinatedBackgroundService
    {
        private readonly IJobFacServiceProvider jobFacServices;

        public TestJobMonitoring(
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices)
            : base(appLifetime)
        {
            this.jobFacServices = jobFacServices;
        }

        protected override async Task ExecuteAsync()
        {
            // Exactly the same as TestJobPayload except the job isn't JobFac-aware

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
                IJobExternalProcess job = null;
                while (!done && DateTimeOffset.UtcNow < timeout)
                {
                    Console.WriteLine("Pausing 10 seconds then reading status.");
                    await Task.Delay(10000);

                    if (job == null) job = jobFacServices.GetExternalProcessJob(jobKey);
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
            finally
            {
                appLifetime.StopApplication();
            }
        }
    }
}
