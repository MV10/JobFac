using JobFac.Library;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    // TODO can SchedulerService be safely marked reentrant with timers firing?
    public class SchedulerService : GrainService, ISchedulerService
    {
        private readonly string grainId;
        private readonly ILogger logger;
        private readonly IClusterClient client;

        public SchedulerService(
            IGrainIdentity id,
            Silo silo,
            ILoggerFactory loggerFactory,
            IClusterClient clusterClient)
            : base(id, silo, loggerFactory)
        {
            grainId = id.IdentityString;
            logger = loggerFactory.CreateLogger(ConstLogging.JobFacCategorySchedulerService);
            client = clusterClient;
        }

        private ISchedulerQueue schedulerQueue;

        public override async Task Init(IServiceProvider serviceProvider)
        {
            logger.LogInformation($"Init");
            schedulerQueue = client.GetGrain<ISchedulerQueue>();
            await base.Init(serviceProvider);
        }

        public override async Task Start()
        {
            logger.LogInformation($"Start");
            await schedulerQueue.SchedulerServiceStarting(grainId);
            await base.Start();
        }

        public override async Task Stop()
        {
            logger.LogInformation($"Stop");
            await schedulerQueue.SchedulerServiceStopping(grainId);
            await base.Stop();
        }
    }
}