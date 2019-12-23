using JobFac.services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobFac.runtime
{
    public class SchedulerSingleton : Grain, ISchedulerSingleton
    {
        public SchedulerSingleton()
        { 
        
        }

    }
}
