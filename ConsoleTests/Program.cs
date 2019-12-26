using JobFac.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);

            await host.AddJobFacClientAsync();
            host.ConfigureServices((ctx, svc) =>
            {
                svc.AddDatabaseServices();

                //svc.AddHostedService<TestDatabaseRepository>();
                svc.AddHostedService<TestJobMonitoring>();
                //svc.AddHostedService<TestJobKilling>();
            });

            await host.RunConsoleAsync();

            if(!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
