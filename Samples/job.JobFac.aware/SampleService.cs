using JobFac.Library;
using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

// See Program.cs for details about the startup payload and
// the expectations of a JobFac-aware job definition.

namespace job.JobFac.aware
{
    public class SampleService : JobFacAwareBackgroundService
    {
        public SampleService(
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices,
            JobFacAwareBackgroundServiceOptions jobFacServiceOptions)
            : base(appLifetime, jobFacServices, jobFacServiceOptions)
        { }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            try
            {
                var payload = jobStartupPayload.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if(payload.Length != 2)
                    throw new ArgumentException("The JobFac-aware sample requires a comma-delimited startup payload with two values");

                if(!int.TryParse(payload[0], out var seconds) || seconds < 1)
                    throw new ArgumentException("The JobFac-aware sample's startup payload requires a value defining number of seconds to sleep.");

                Console.WriteLine($"Sample sleeping for {seconds} secs in 5 second increments.");
                Console.WriteLine($"Second startup payload value is: {payload[1]}");

                var exitAt = DateTimeOffset.Now.AddSeconds(seconds);
                int counter = 0;
                while (exitAt > DateTimeOffset.Now)
                {
                    appStoppingToken.ThrowIfCancellationRequested();
                    Console.WriteLine($"\nSleep interval #{++counter}");
                    for (int i = 0; i < 5; i++)
                    {
                        Console.Write(".");
                        await Task.Delay(1000);
                    }
                }

                Console.WriteLine("\n\nWriting \"OK\" to stderr.");
                Console.Error.WriteLine("OK"); // also emitted to stdout

                Environment.ExitCode = 12345;
                Console.WriteLine($"\nSample set exit code {Environment.ExitCode}");

                Console.WriteLine("Writing final exit status and message to JobFac.");
                await jobService.UpdateExitMessage(RunStatus.Ended, Environment.ExitCode, payload[1]);
            }
            catch(Exception ex)
            {
                Environment.ExitCode = -1;
                Console.Error.WriteLine(ex);
                if (jobService != null)
                    await jobService.UpdateExitMessage(RunStatus.Failed, -1, ex.ToString());
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }
    }
}
