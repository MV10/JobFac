using JobFac.database;
using JobFac.lib.DataModels;
using JobFac.services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTests
{
    class Program
    {
        static IServiceProvider services;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Reading config and building service provider.");
            services = AddConfiguration()
                .AddDatabaseServices()
                .BuildServiceProvider();

            //await TestGetJobDefinition();
            await TestStartJob();

            if(!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit.");
                Console.ReadKey(true);
            }
        }

        static async Task TestStartJob()
        {
            Console.WriteLine("Getting cluster client.");
            var client = await GetClusterClient();

            try
            {
                Console.WriteLine("Getting job factory proxy.");
                var factory = client.GetGrain<IJobFactory>(0);

                var options = new FactoryStartOptions
                {
                    DefinitionId = "Sample.JobFac.unaware",
                    NotificationScope = NotificationScope.None,
                    ReplacementArguments = new System.Collections.Generic.Dictionary<string, string>(),
                    StartupPayloads = new System.Collections.Generic.Dictionary<string, string>()
                };

                Console.WriteLine("Starting 45-second job: Sample.JobFac.unaware");
                var jobKey = await factory.StartJob(options);
                Console.WriteLine($"Job instance key: {jobKey}");

                //await TestMonitorJob();
                await TestKillJob();

                async Task TestKillJob()
                {
                    Console.WriteLine("Waiting 25 seconds then will kill job.");
                    await Task.Delay(25000);
                    var job = client.GetGrain<IJob>(jobKey);
                    if (job != null)
                    {
                        var status = await job.GetStatus();
                        Console.WriteLine($"Status {status.RunStatus} last updated {status.LastUpdated.ToLocalTime()}");
                        Console.WriteLine("Killing job then sleepint for 5 seconds.");
                        await job.Stop();
                        await Task.Delay(5000);
                        status = await job.GetStatus();
                        Console.WriteLine($"Status {status.RunStatus} last updated {status.LastUpdated.ToLocalTime()}");
                    }
                    else
                        Console.WriteLine("Failed to obtain job proxy.");
                }

                async Task TestMonitorJob()
                {
                    var timeout = DateTimeOffset.UtcNow.AddSeconds(90);
                    bool done = false;
                    IJob job = null;
                    while (!done && DateTimeOffset.UtcNow < timeout)
                    {
                        Console.WriteLine("Pausing 10 seconds then reading status.");
                        await Task.Delay(10000);

                        if (job == null) job = client.GetGrain<IJob>(jobKey);
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

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\nException {ex}\n\n");
            }
            finally
            {
                Console.WriteLine("Closing cluster client.");
                await client.Close();
                client.Dispose();
            }
        }

        static async Task TestGetJobDefinition()
        {
            var defRepo = services.GetRequiredService<DefinitionsRepository>();
            Console.WriteLine("Querying repository.");
            var jobDef = await defRepo.GetJobDefinition("Sample.JobFac.unaware");
            Console.WriteLine($"Response is null? {jobDef is null}");
            if (jobDef is null) return;

            Console.WriteLine($"ExecutablePathname:\n{jobDef.ExecutablePathname}");
        }

        static IServiceCollection AddConfiguration()
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true, true)
              .Build();

            var servicesBuilder = new ServiceCollection()
                .AddSingleton((IConfiguration)configuration);

            return servicesBuilder;
        }

        // cribbed from JobFac.runner
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
