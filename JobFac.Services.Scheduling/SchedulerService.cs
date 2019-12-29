using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    [Reentrant]
    public class SchedulerService : GrainService, ISchedulerService
    {
        public SchedulerService()
        {
        }
    }
}
