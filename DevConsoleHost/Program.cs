using JobFac.Library.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Diagnostics;
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

                host.ConfigureLogging(builder =>
                {
                    // TODO move logging filters to appconfig.json
                    builder

                    .SetMinimumLevel(LogLevel.Warning)
                    //.AddFilter("Microsoft", LogLevel.Warning)   // generic host lifecycle messages
                    //.AddFilter("Orleans", LogLevel.Warning)     // suppress status dumps
                    //.AddFilter("Runtime", LogLevel.Warning)     // also an Orleans prefix

                    // JobFac Runner currently uses info, error, and trace
                    //.AddFilter(ConstLogging.JobFacLoggerProviderName, LogLevel.Trace)

                    .AddDebug() // VS Debug window
                    .AddConsole();
                });

                host.UseOrleans(builder =>
                {
                    builder

                    // TODO use real configuration
                    .UseLocalhostClustering() // cluster and service IDs default to "dev"

                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                    .AddJobFacRuntimeParts();
                });

                host.ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();

                    services.AddSingleton(hostContext.Configuration); // required by database services
                    services.AddDatabaseServices();
                });

                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
