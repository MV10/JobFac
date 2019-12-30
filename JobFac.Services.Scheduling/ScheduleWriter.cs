using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class ScheduleWriter : Grain, IScheduleWriter
    {
        public Task UpdateJobSchedules(string jobDefinitionId)
        {
            throw new NotImplementedException();
        }

        public Task WriteNewScheduleTargets()
        {
            throw new NotImplementedException();
        }
    }
}
