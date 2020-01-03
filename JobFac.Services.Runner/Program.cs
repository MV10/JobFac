using JobFac.Library;
using JobFac.Library.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JobFac.Services.Runner
{
    public class Program
    {
        public static string JobInstanceKey { get; set; } = string.Empty;

        public static async Task Main(string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("Runner requires one GUID argument reflecting the job instance-key");

            JobInstanceKey = Formatting.FormattedInstanceKey(args[0]);
            if (!JobInstanceKey.HasContent())
                throw new ArgumentException("Runner requires one GUID argument reflecting the job instance-key");

            var host = Host.CreateDefaultBuilder(args);

            host.ConfigureLogging(builder => 
            {
                builder
                .SetMinimumLevel(LogLevel.Warning)

                // Redirects logs to Orleans silo logger, overrides JobFac prefix with 
                // Trace level so all JobFac messages will pass through to remote logger
                // where they can be filtered by configuration at any arbitrary LogLevel
                .AddJobFacRemoteLogger(); 
            });

            // JobFacLoggerProvider requires IClusterClient in DI
            await host.AddJobFacClientAsync(addIClusterClient: true);

            host.ConfigureServices((ctx, svc) =>
            {
                svc.AddLogging();
                svc.AddHostedService<ProcessMonitor>();
            });

            // TODO change this when Generic Host issue 1363 is fixed
            // Required to dispose IHost, see https://github.com/aspnet/Extensions/issues/1363
            // Calling "await host.StartAsync()" hangs the process in memory until 1363 fixed
            using var ihost = host.Build();
            await ihost.RunAsync();
        }
    }
}
