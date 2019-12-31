using JobFac.Library;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

namespace JobFac.Services.Scheduling
{
    // Typically it's recommended to mark a GrainService as [Reentrant]
    // if possible, but this only makes outbound calls (to SchedulerQueue).

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
        private IDisposable timerHandle = null;

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
            timerHandle = RegisterTimer(async (_) => { await RequestNewWork(); }, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMinutes(1));
            await base.Start();
        }

        public override async Task Stop()
        {
            timerHandle?.Dispose();
            timerHandle = null;
            logger.LogInformation($"Stop");
            await schedulerQueue.SchedulerServiceStopping(grainId);
            await base.Stop();
        }

        public async Task RequestNewWork()
        {
            timerHandle?.Dispose();
            timerHandle = null;

            while (timerHandle == null)
            {
                var assignedJobs = await schedulerQueue.GetJobAssignments(grainId);
                if (assignedJobs.Count == 0)
                {
                    IdleUntilNextMinute();
                }
                else
                {
                    logger.LogInformation($"Executing {assignedJobs.Count} jobs");
                    foreach(var job in assignedJobs)
                    {
                        var execKey = job.DefinitionId + job.ScheduleTarget.ToString();
                        var execGrain = client.GetGrain<IScheduledExecution>(execKey);
                        execGrain.StartScheduledJob(job).Ignore();
                    }
                    assignedJobs = null;
                }
            }
        }

        public void IdleUntilNextMinute()
        {
            var delay = 1000 - DateTimeOffset.Now.Millisecond;
            timerHandle = RegisterTimer(async (_) => { await RequestNewWork(); }, null, TimeSpan.FromMilliseconds(delay), TimeSpan.FromMinutes(1));
            logger.LogInformation($"Delaying {delay} ms");
        }
    }
}
