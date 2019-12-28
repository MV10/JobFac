using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestJobPayload : CoordinatedBackgroundService
    {
        private readonly IJobFacServiceProvider jobFacServices;

        public TestJobPayload(
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices)
            : base(appLifetime)
        {
            this.jobFacServices = jobFacServices;
        }

        protected override async Task ExecuteAsync()
        {
            // Exactly the same as TestJobMonitoring except the job is JobFac-aware and needs a payload

            try
            {
                Console.WriteLine("Getting job factory proxy.");
                var factory = jobFacServices.GetJobFactory();

                var options = new FactoryStartOptions
                {
                    DefinitionId = "Sample.JobFac.aware"
                    // we could add the payload here but it's easier to use the StartJob overload
                    // since FactoryStartOptions stores alternate argument lists and startup
                    // payloads in a dictionary (which is useful for multi-job sequences)
                };

                string payload = "35,Hello world!";

                Console.WriteLine($"Starting sample job Sample.JobFac.aware with payload: {payload}");
                var jobKey = await factory.StartJob(options, startupPayload: payload);

                // Everything below is identical to TestJobMonitoring
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
            catch (Exception ex)
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
