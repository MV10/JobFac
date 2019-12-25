using JobFac.Library.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public interface IJob : IGrainWithStringKey
    {
        Task Start(JobDefinition jobDefinition, FactoryStartOptions options);
        Task<JobDefinition> GetDefinition();
        Task<JobStatus> GetStatus();
        Task UpdateRunStatus(RunStatus runStatus);
        Task UpdateExitMessage(RunStatus runStatus, int exitCode, string exitMessage);
        Task Stop();
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
    }
}
