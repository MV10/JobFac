using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            await host.AddJobFacClientAsync();
            host.ConfigureServices((ctx, svc) => svc.AddHostedService<ProcessMonitor>());
            await host.RunConsoleAsync();
        }
    }
}
