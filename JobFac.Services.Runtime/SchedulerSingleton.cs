using JobFac.Services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobFac.Services.Runtime
{
    public class SchedulerSingleton : Grain, ISchedulerSingleton
    {
        public SchedulerSingleton()
        { 
        
        }

    }
}
