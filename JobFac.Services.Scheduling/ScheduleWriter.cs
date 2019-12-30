using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class ScheduleWriter : Grain, IScheduleWriter
    {
        public async Task UpdateJobSchedules(string jobDefinitionId)
        {
            
        }

        public async Task WriteNewScheduleTargets()
        {

        }
    }
}
