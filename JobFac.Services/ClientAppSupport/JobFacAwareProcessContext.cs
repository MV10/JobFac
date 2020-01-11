using JobFac.Library.DataModels;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class JobFacAwareProcessContext
    {
        public string JobInstanceId { get; private set; } = string.Empty;
        public string[] CommandLineArgs { get; private set; } = new string[0];
        public string StartupPayload { get; private set; } = string.Empty;
        public IJobExternalProcess JobService { get; private set; } = null;

        private readonly IHostApplicationLifetime appLifetime;

        public JobFacAwareProcessContext(
            IHostApplicationLifetime hostApplicationLifetime,
            string instanceId, string[] args, string payload,
            IJobExternalProcess jobService)
        {
            appLifetime = hostApplicationLifetime;
            JobInstanceId = instanceId;
            CommandLineArgs = args;
            StartupPayload = payload;
            JobService = jobService;
        }

        public async Task ExitAsync(string exitMessage = "", int? exitCode = null)
        {
            try
            {
                await JobService.UpdateExitMessage(RunStatus.Ended, exitCode ?? Environment.ExitCode, exitMessage);
            }
            catch
            {
                // if UpdateExitMessage fails, there isn't anything else to try
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }

        public async Task ExitFailedAsync(Exception exception, int? exitCode = null)
        {
            try
            {
                await JobService.UpdateExitMessage(RunStatus.Failed, exitCode ?? Environment.ExitCode, exception.ToString());
            }
            catch
            {
                // if UpdateExitMessage fails, there isn't anything else to try
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }

        public async Task ExitFailedAsync(string exitMessage = "", int? exitCode = null)
        {
            try
            {
                await JobService.UpdateExitMessage(RunStatus.Failed, exitCode ?? Environment.ExitCode, exitMessage);
            }
            catch
            {
                // if UpdateExitMessage fails, there isn't anything else to try
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }

    }
}
