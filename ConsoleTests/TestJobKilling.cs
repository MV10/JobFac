using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestJobKilling : CoordinatedBackgroundService
    {
        private readonly IJobFacServiceProvider jobFacServices;

        public TestJobKilling(
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices)
            : base(appLifetime)
        {
            this.jobFacServices = jobFacServices;
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
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

                Console.WriteLine("Waiting 15 seconds then will kill job.");
                await Task.Delay(15000);
                var job = jobFacServices.GetExternalProcessJob(jobKey);
                if (job != null)
                {
                    var status = await job.GetStatus();
                    Console.WriteLine($"Status {status.RunStatus} last updated {status.LastUpdated.ToLocalTime()}");
                    Console.WriteLine("Killing job then sleeping for 5 seconds.");
                    await job.Stop();
                    await Task.Delay(5000);
                    status = await job.GetStatus();
                    Console.WriteLine($"Status {status.RunStatus} last updated {status.LastUpdated.ToLocalTime()}");
                }
                else
                    Console.WriteLine("Failed to obtain job proxy.");
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