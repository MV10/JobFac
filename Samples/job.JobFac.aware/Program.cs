using JobFac.Library;
using JobFac.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Configuring a job as JobFac-aware implies:
// * first command-line argument to job is the instance key
// * can call IJob service to pick up more detailed payload
// * must call IJob.UpdateExitMessage prior to exiting

// A startup payload can be any string value, so it could, for example,
// represent a JSON-serialized object. For this sample, we simply require
// a pair of comma-separated values. The first value is the number of
// seconds to sleep, and the second value is written to the console and
// written as the job exit message upon successful completion.

// Example payload for this sample: 25,Hello World!

namespace job.JobFac.aware
{
    class Program
    {
        public static string JobInstanceKey;

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            JobInstanceKey = Formatting.FormattedInstanceKey(args[0]);
            if (!JobInstanceKey.HasContent())
                throw new Exception("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            var host = Host.CreateDefaultBuilder(args);
            host.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            await host.AddJobFacClientAsync();
            host.ConfigureServices(svc => svc.AddHostedService<SampleService>());
            await host.RunConsoleAsync();
        }
    }
}
