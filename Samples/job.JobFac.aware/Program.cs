
#define USE_JOBFAC_DEV_MODE
#if RELEASE
    #undef USE_JOBFAC_DEV_MODE
#endif

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Configuring a job as JobFac-aware implies:
//
// * first command-line argument to job is the instance key
// * can call IJob service to pick up more detailed payload
// * must call IJob.UpdateExitMessage prior to exiting (including upon crashing)
//
// All of these responsibilities are handed by JobFacAwareProcessContext, the
// actual job only needs to drive from JobFacAwareProcessBase. Otherwise the
// job would have to implement everything in JobFacAwareBackgroundService, too.
//
// A startup payload can be any string value, so it could, for example,
// represent a JSON-serialized object. For this sample, we simply require
// a pair of comma-separated values. The first value is the number of
// seconds to sleep, and the second value is written to the console and
// written as the job exit message upon successful completion.
//
// Example payload for this sample: 
// 25,Hello World!

namespace job.JobFac.aware
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);
            host.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

#if USE_JOBFAC_DEV_MODE
            // Extra precaution since our JobFac definitions for the sample job
            // do redirect output, indicating dev mode was accidentally left
            // enabled. For the purposes of this sample code, this also allows
            // you to run this job interactively, or to launch the sample via
            // JobFac without having to reference a separate Release build. No
            // command-line instance ID is required for dev mode usage, simply
            // specify the job's internal arguments (the sample needs none).
            if(!Console.IsOutputRedirected)
                host.UseJobFacAwareDevelopmentMode(startupPayload:"10,Hello World!");
#endif

            await host.AddJobFacAwareServicesAsync<SampleService>(options =>
            {
                options.CommandLineArgs = args;
                options.RetrieveStartupPayload = true;
                options.FailJobOnMissingStartupPayload = true;
            });

            await host.RunConsoleAsync();
        }
    }
}
