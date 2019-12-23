using JobFac.lib.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.services
{
    public interface IJob : IGrainWithStringKey
    {
        Task Start(JobDefinition jobDefinition, FactoryStartOptions options);
        Task<JobDefinition> GetDefinition();
        Task<JobStatus> GetStatus();
        Task UpdateRunStatus(RunStatus runStatus);
        Task UpdateExitMessage(RunStatus runStatus, string exitMessage);
        Task Stop();
    }
}
