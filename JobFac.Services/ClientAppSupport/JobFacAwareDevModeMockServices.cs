using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using System;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class MockJobExternalProcess : IJobExternalProcess
    {
        public Task<JobDefinition<DefinitionExternalProcess>> GetDefinition()
            => Task.FromResult(new JobDefinition<DefinitionExternalProcess>());

        public Task<string> GetStartupPayload()
            => Task.FromResult(string.Empty);

        public Task<JobStatus<StatusExternalProcess>> GetStatus()
            => Task.FromResult(new JobStatus<StatusExternalProcess>());

        public Task Start(JobDefinition<DefinitionExternalProcess> jobDefinition, FactoryStartOptions options)
            => Task.CompletedTask;

        public Task Stop()
            => Task.CompletedTask;

        public Task UpdateExitMessage(RunStatus runStatus, int exitCode, string exitMessage)
        {
            Console.WriteLine($"JobFac dev-mode UpdateExitMessage:\n  Run Status: {runStatus}\n  Exit Code: {exitCode}\n  Exit Message: {exitMessage}");
            return Task.CompletedTask;
        }

        public Task UpdateRunStatus(RunStatus runStatus)
        {
            Console.WriteLine($"JobFac dev-mode UpdateExitMessage:\n  Run Status: {runStatus}");
            return Task.CompletedTask;
        }

        public Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr)
            => Task.CompletedTask;
    }

    public class MockJobServiceProvider : IJobFacServiceProvider
    {
        public async ValueTask DisposeAsync()
        { }

        public IJobExternalProcess GetExternalProcessJob(string instanceId)
            => throw new NotImplementedException();

        public IJobFactory GetJobFactory()
            => throw new NotImplementedException();
    }
}
