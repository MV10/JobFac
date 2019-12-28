using JobFac.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);

                host.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

                // CTRL+C support, although that won't stop the Runner or job
                // TODO does CTRL+C here cause the Runner to *never* unload?
                host.UseConsoleLifetime();

                await host.AddJobFacClientAsync();
                host.ConfigureServices((ctx, svc) =>
                {
                    svc.AddDatabaseServices();

                    //svc.AddHostedService<TestDatabaseRepository>();
                    //svc.AddHostedService<TestJobMonitoring>();
                    //svc.AddHostedService<TestJobKilling>();
                    svc.AddHostedService<TestJobPayload>();
                });

                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if(!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
