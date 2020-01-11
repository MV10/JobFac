using JobFac.Library;
using JobFac.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public static class AddJobFacClientGenericHostExtension
    {
        private static readonly string DEVMODE_PAYLOAD = "JOBFAC_DEVPAYLOAD";

        public static async Task<IHostBuilder> AddJobFacAwareServicesAsync<TJobFacAwareProcess>(this IHostBuilder hostBuilder, Action<JobFacAwareProcessOptions> optionsDelegate)
            where TJobFacAwareProcess : JobFacAwareProcessBase
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<TJobFacAwareProcess>();
                
                JobFacAwareProcessOptions options = new JobFacAwareProcessOptions();
                optionsDelegate(options);

                if(!hostBuilder.Properties.ContainsKey(DEVMODE_PAYLOAD))
                {
                    services.AddHostedService<JobFacAwareBackgroundService<TJobFacAwareProcess>>();
                }
                else
                {
                    options.DevModeStartupPayload = (string)hostBuilder.Properties[DEVMODE_PAYLOAD];
                    services.AddHostedService<JobFacAwareDevelopmentModeService<TJobFacAwareProcess>>();
                }

                services.AddSingleton(options);
            });

            if (!hostBuilder.Properties.ContainsKey(DEVMODE_PAYLOAD))
                await hostBuilder.AddJobFacClientAsync();

            return hostBuilder;
        }

        public static IHostBuilder UseJobFacAwareDevelopmentMode(this IHostBuilder hostBuilder, string startupPayload = "")
        {
            Console.WriteLine("JobFac dev-mode enabled");
            hostBuilder.Properties.Add(DEVMODE_PAYLOAD, startupPayload);
            return hostBuilder;
        }

        public static async Task<IHostBuilder> AddJobFacClientAsync(this IHostBuilder hostBuilder, bool addIClusterClient = false)
        {
            // Nasty bit of code to "borrow" a copy of HostBulderContext so we can use it with await
            // instead of followng the normal ConfigureServices lambda pattern, like this:
            //
            // builder.ConfigureServices(async (context, services) => 
            //                           ^^^^^
            // {
            //      var client = await GetConnectedClient(hostContext.Configuration); 
            //                   ^^^^^
            //      services.AddSingleton(client);
            // });
            //
            // An async lambda is async void, so no Task is returned that can be awaited.
            IConfiguration config = null;
            hostBuilder.ConfigureServices((context, services) => config = context.Configuration );

            var orleansClient = await GetConnectedClient(config);
            if (orleansClient is null)
                throw new JobFacConnectivityException("Unable to connect to JobFac cluster");

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IJobFacServiceProvider>(new JobFacServiceProvider(orleansClient));
                services.AddHostedService<ClusterClientHostedService>();
                if (addIClusterClient) services.AddSingleton(orleansClient);
            });

            return hostBuilder;
        }

        private static async Task<IClusterClient> GetConnectedClient(IConfiguration config)
        {
            int remainingAttempts = 3; // TODO read retry attempts and delay from context.config
            int retryDelaySeconds = 15;
            IClusterClient client = null;
            while (remainingAttempts-- > 0 && client is null)
            {
                try
                {
                    client = await TryConnect(config);
                }
                catch
                {
                    if (remainingAttempts == 0) throw;
                    await Task.Delay(retryDelaySeconds * 1000).ConfigureAwait(false);
                }
            }
            return client;
        }

        private static async Task<IClusterClient> TryConnect(IConfiguration config)
        {
            IClusterClient client = null;

            try
            {
                var builder = new ClientBuilder();

                //builder.ConfigureLogging(logging => {
                //    logging
                //    .AddFilter("Microsoft", LogLevel.Warning)
                //    .AddFilter("Orleans", LogLevel.Warning)
                //    .AddFilter("Runtime", LogLevel.Warning)
                //    .AddConsole();
                //})

                // TODO read configuration
                builder.UseLocalhostClustering(); // cluster and service IDs default to "dev"

                builder.AddJobFacServicesParts();
                client = builder.Build();

                // causes host builder to run hosted services???
                await client.Connect().ConfigureAwait(false);
            }
            catch
            {
                client?.Dispose();
                throw;
            }

            return client;
        }
    }
}
