using JobFac.Library.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public class ScheduledExecution : Grain, IScheduledExecution
    {
        public Task StartScheduledJob(PendingScheduledJob assignment)
        {
            throw new System.NotImplementedException();
        }
    }
}
