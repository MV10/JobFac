using JobFac.runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DevConsoleHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);

                host.UseOrleans(builder =>
                {
                    builder

                    // TODO use real configuration
                    .UseLocalhostClustering() // cluster and service IDs default to "dev"

                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                    .AddJobFacRuntimeParts();
                });

                host.ConfigureLogging(builder =>
                {
                    builder
                    .AddFilter("Microsoft", LogLevel.Warning)   // generic host lifecycle messages
                    .AddFilter("Orleans", LogLevel.Warning)     // suppress status dumps
                    .AddFilter("Runtime", LogLevel.Warning)     // also an Orleans prefix
                    .AddDebug()                                 // VS Debug window
                    .AddConsole();
                });

                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(hostContext.Configuration);
                    services.AddLogging();
                    services.AddDatabaseServices();
                });

                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
