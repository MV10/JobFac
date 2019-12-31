using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services.Scheduling
{
    public interface IScheduleWriter : IClusterSingletonGrain
    {
        Task WriteNewScheduleTargets();
        Task UpdateJobSchedules(string jobDefinitionId, bool removeExistingRows = true);
    }
}
