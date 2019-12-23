using JobFac.lib.DataModels;
using JobFac.services;
using Orleans;
using System;
using System.Threading.Tasks;

namespace JobFac.runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
                throw new Exception("Missing required job instance ID argument");

            var jobId = args[0];
            var clusterClient = await GetOrleansClusterClient();
            var jobService = clusterClient.GetGrain<IJob>(jobId);
            try
            {
                var jobDef = await jobService.GetDefinition();
                await RunJob(jobService, jobDef);
            }
            catch(Exception ex)
            {

            }
            finally
            {
                clusterClient?.Close();
                clusterClient?.Dispose();
            }
        }

        static async Task RunJob(IJob jobService, JobDefinition jobDef)
        {

        }

        static async Task<IClusterClient> GetOrleansClusterClient()
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
                
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(IJob).Assembly).WithReferences();
                })
                .Build();

            // TODO handle exceptions and add retry logic
            await client.Connect();

            return client;
        }
    }
}
