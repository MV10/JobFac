using JobFac.Library.Database;
using JobFac.Library.Constants;
using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Microsoft.Extensions.Configuration;
using Orleans;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading.Tasks;

namespace JobFac.Services.Runtime
{
    public class JobExternalProcess : Grain, IJobExternalProcess
    {
        private string jobInstanceKey = null;
        private JobStatus<StatusExternalProcess> status = null;
        private JobDefinition<DefinitionExternalProcess> jobDefinition = null;

        private readonly HistoryRepository historyRepo;
        private readonly string runnerExecutablePathname;

        public JobExternalProcess(HistoryRepository history, IConfiguration configuration)
        {
            historyRepo = history;
            runnerExecutablePathname = configuration.GetValue<string>(ConstConfigKeys.RunnerExecutablePathname);
        }

        public override async Task OnActivateAsync()
        {
            jobInstanceKey = this.GetPrimaryKeyString();
            var history = await historyRepo.GetJobHistory(jobInstanceKey);
            if(history != null) status = historyRepo.DeserializeDetails<StatusExternalProcess>(history);
        }

        public async Task Start(JobDefinition<DefinitionExternalProcess> jobDefinition, FactoryStartOptions options)
        {
            if (status != null)
                throw new Exception($"Job has already been started (instance {jobInstanceKey})");

            jobDefinition.ThrowIfInvalid();

            this.jobDefinition = jobDefinition;

            status = new JobStatus<StatusExternalProcess>
            {
                Key = jobInstanceKey,
                StartOptions = options,
                LastUpdated = DateTimeOffset.UtcNow,
            };
            status.JobTypeProperties.MachineName = Dns.GetHostName().HasContent() ? Dns.GetHostName() : Environment.MachineName;
            await historyRepo.InsertStatus(status);

            await LaunchRunner();

            // TODO add startup timeout to check whether RunStatus changes from Unknown (requires grain timer support)
        }

        public Task<JobDefinition<DefinitionExternalProcess>> GetDefinition()
        {
            if (jobDefinition == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            return Task.FromResult(jobDefinition);
        }

        public Task<JobStatus<StatusExternalProcess>> GetStatus()
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            return Task.FromResult(status);
        }

        public Task<string> GetStartupPayload()
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            status.StartOptions.StartupPayloads.TryGetValue(jobDefinition.Id, out var payload);
            return Task.FromResult(payload);
        }

        public async Task UpdateExitMessage(RunStatus runStatus, int exitCode, string exitMessage)
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            if (status.HasExited)
                throw new Exception($"Job has already exited (instance {jobInstanceKey})");

            status.JobTypeProperties.ExitCode = exitCode;
            status.JobTypeProperties.ExitMessage = exitMessage;
            await StoreNewRunStatus(runStatus);
        }

        public async Task UpdateRunStatus(RunStatus runStatus)
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            if (status.HasExited)
                throw new Exception($"Job has already exited (instance {jobInstanceKey})");

            await StoreNewRunStatus(runStatus);
        }

        public async Task Stop()
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            if (status.RunStatus != RunStatus.Running)
                throw new Exception($"Job is not in Running status (instance {jobInstanceKey})");

            if (status.HasExited)
                throw new Exception($"Job has already exited (instance {jobInstanceKey})");

            await StoreNewRunStatus(RunStatus.StopRequested);

            // Simply connecting to the runner's named-pipe is the signal to kill the child process.
            using var client = new NamedPipeClientStream(".", jobInstanceKey, PipeDirection.Out, PipeOptions.Asynchronous);
            await client.ConnectAsync(ConstTimeouts.NamedPipeClientConnectMS);
        }

        public async Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr)
        {
            var dataUtil = GrainFactory.GetGrain<IDataUtilities>(0); // StatelessWorker
            await dataUtil.WriteCapturedOutput(instanceKey, stdOut, stdErr);
        }

        private async Task LaunchRunner()
        {
            var proc = new Process();
            try
            {
                proc.StartInfo.FileName = runnerExecutablePathname;
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(runnerExecutablePathname);
                proc.StartInfo.Arguments = jobInstanceKey;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                if (!proc.Start())
                {
                    await UpdateExitMessage(RunStatus.StartFailed, -1, "Unable to start JobFac runner");
                    return;
                }
            }
            finally
            {
                proc?.Close(); // releases resources but does not terminate process
                proc?.Dispose();
            }
        }

        private async Task StoreNewRunStatus(RunStatus runStatus)
        {
            // TODO notifications

            var now = DateTimeOffset.UtcNow;
            status.LastUpdated = now;
            status.RunStatus = runStatus;
            switch (runStatus)
            {
                case RunStatus.StartRequested:
                    status.StartRequested = now;
                    break;

                case RunStatus.StartFailed:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Running:
                    status.HasStarted = true;
                    break;

                case RunStatus.Stopped:
                    status.HasFailed = true;
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Ended:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Failed:
                    status.HasFailed = true;
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                // JobFac.Services.Runner faulted while job was still running
                case RunStatus.Unknown:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;
            }

            await historyRepo.UpdateStatus(status);
        }
    }
}
