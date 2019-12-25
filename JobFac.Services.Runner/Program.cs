using JobFac.Library.DataModels;
using JobFac.Services;
using Orleans;
using System;
using System.Threading.Tasks;

namespace JobFac.Services.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
                throw new Exception("Runner requires one argument reflecting the job instance-key");

            var jobKey = args[0];
            if (!Guid.TryParse(jobKey, out var _))
                throw new Exception($"Job instance-key is invalid, format-D GUID expected");

            var clusterClient = await GetClusterClient();
            var jobService = clusterClient.GetGrain<IJob>(jobKey);
            if (jobService == null)
                throw new Exception($"Unable to connect to job service (instance {jobKey}");

            try
            {
                await jobService.UpdateRunStatus(RunStatus.StartRequested);
                var jobDef = await jobService.GetDefinition();
                await ProcessMonitor.RunJob(jobService, jobDef, jobKey);
            }
            catch(Exception ex)
            {
                await jobService.UpdateExitMessage(RunStatus.Unknown, -1, $"JobFac.Services.Runner exception {ex}");
            }
            finally
            {
                await clusterClient.Close();
                clusterClient.Dispose();
            }
        }

        static async Task<IClusterClient> GetClusterClient()
        {
            var client = new ClientBuilder()
                //.ConfigureLogging(logging => {
                //    logging
                //    .AddFilter("Microsoft", LogLevel.Warning)
                //    .AddFilter("Orleans", LogLevel.Warning)
                //    .AddFilter("Runtime", LogLevel.Warning)
                //    .AddConsole();
                //})

                // TODO read configuration
                .UseLocalhostClustering() // cluster and service IDs default to "dev"

                .AddJobFacServicesParts()

                .Build();

            // TODO handle exceptions and add retry logic
            await client.Connect();

            return client;
        }
    }
}
