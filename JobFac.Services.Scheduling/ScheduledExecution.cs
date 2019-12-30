using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class ScheduledExecution : Grain, IScheduledExecution
    {
        public Task StartScheduledJob(JobAssignment assignment)
        {
            throw new System.NotImplementedException();
        }
    }
}
