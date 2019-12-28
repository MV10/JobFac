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
                throw new Exception("Runner requires one argument reflecting the job instance-key");

            JobInstanceKey = args[0];
            if (!Guid.TryParse(JobInstanceKey, out var _))
                throw new Exception($"Job instance-key is invalid, format-D GUID expected");

            var host = Host.CreateDefaultBuilder(args);

            host.ConfigureLogging(builder => 
            {
                // TODO move logging filters to appconfig.json
                builder
                .SetMinimumLevel(LogLevel.Warning)

                // redirect logs to Orleans silo logger, overrides JobFac prefix with 
                // Trace level so all JobFac messages will pass through to remote logger
                .AddJobFacRemoteLogger(); 
            });

            // JobFacLoggerProvider requires IClusterClient in DI
            await host.AddJobFacClientAsync(addIClusterClient: true);

            host.ConfigureServices((ctx, svc) =>
            {
                svc.AddLogging();
                svc.AddHostedService<ProcessMonitor>();
            });

            // required to dispose IHost, see https://github.com/aspnet/Extensions/issues/1363
            // await host.StartAsync(); this will cause process to hang in memory until 1363 fixed
            using var ihost = host.Build();
            await ihost.RunAsync();
        }
    }
}
