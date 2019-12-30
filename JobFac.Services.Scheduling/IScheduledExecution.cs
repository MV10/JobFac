using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public interface IScheduledExecution : IGrainWithStringKey
    {
        Task StartScheduledJob(JobAssignment assignment);
    }
}
