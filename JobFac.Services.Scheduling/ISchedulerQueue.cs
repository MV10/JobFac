﻿using JobFac.Library.DataModels;
using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public interface ISchedulerQueue : IClusterSingletonGrain
    {
        Task SchedulerServiceStarting(string schedulerServiceId);
        Task SchedulerServiceStopping(string schedulerServiceId);
        Task<IReadOnlyList<PendingScheduledJob>> GetJobAssignments(string schedulerServiceId);
    }
}
