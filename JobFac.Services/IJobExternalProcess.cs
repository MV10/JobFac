using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public interface IJobExternalProcess : IGrainWithStringKey
    {
        Task Start(JobDefinition<DefinitionExternalProcess> jobDefinition, FactoryStartOptions options);
        Task<JobDefinition<DefinitionExternalProcess>> GetDefinition();
        Task<JobStatus<StatusExternalProcess>> GetStatus();
        Task<string> GetStartupPayload();
        Task UpdateRunStatus(RunStatus runStatus);
        Task UpdateExitMessage(RunStatus runStatus, int exitCode, string exitMessage);
        Task Stop();
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
    }
}
