using JobFac.Library;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public class JobFacAwareBackgroundService<TJobFacAwareProcess> : CoordinatedBackgroundService
        where TJobFacAwareProcess : JobFacAwareProcessBase
    {
        private readonly TJobFacAwareProcess process;
        private readonly IJobFacServiceProvider jobFacProvider;
        private readonly JobFacAwareProcessOptions options;
        private readonly string jobInstanceId;

        public JobFacAwareBackgroundService(
            TJobFacAwareProcess targetProcess,
            IHostApplicationLifetime hostApplicationLifetime,
            IJobFacServiceProvider jobFacServiceProvider,
            JobFacAwareProcessOptions jobFacServiceOptions)
            : base(hostApplicationLifetime)
        {
            if (jobFacServiceOptions.CommandLineArgs.Length == 0)
                throw new ArgumentException("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            jobInstanceId = Formatting.FormattedInstanceKey(jobFacServiceOptions.CommandLineArgs[0]);
            if (!jobInstanceId.HasContent())
                throw new ArgumentException("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            process = targetProcess;
            jobFacProvider = jobFacServiceProvider;
            options = jobFacServiceOptions;
        }

        private JobFacAwareProcessContext jobFacContext;

        protected override async Task InitializingAsync(CancellationToken cancelInitToken)
        {
            IJobExternalProcess service = null;
            try
            {
                var args = options.CommandLineArgs.Length > 1 ? options.CommandLineArgs[1..^1] : new string[0];
                if (!process.ValidateArguments(args))
                    throw new ArgumentException($"Validation of the command-line arguments failed (instance {jobInstanceId})");

                service = jobFacProvider.GetExternalProcessJob(jobInstanceId);
                if (service == null)
                    throw new JobFacConnectivityException($"Unable to connect to job service (instance {jobInstanceId})");

                cancelInitToken.ThrowIfCancellationRequested();

                string payload;
                if (options.RetrieveStartupPayload)
                {
                    payload = await service.GetStartupPayload();
                    if (!payload.HasContent())
                    {
                        if (options.FailJobOnMissingStartupPayload)
                            throw new ArgumentException("The JobFac-aware process requires a startup payload");

                        payload = string.Empty;
                    }

                    if(!process.ValidateStartupPayload(payload))
                        throw new ArgumentException($"Validation of the startup payload failed (instance {jobInstanceId})");
                }
                else
                {
                    payload = string.Empty;
                }

                cancelInitToken.ThrowIfCancellationRequested();

                jobFacContext = new JobFacAwareProcessContext(appLifetime, jobInstanceId, args, payload, service, jobFacProvider);

                await base.InitializingAsync(cancelInitToken);
            }
            catch (Exception ex)
            {
                if (service != null)
                    await service.UpdateExitMessage(RunStatus.Failed, options.ExceptionExitCode, ex.ToString()).ConfigureAwait(false);
                
                appLifetime.StopApplication();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            try
            {
                await process.ExecuteProcessingAsync(jobFacContext, appStoppingToken);
            }
            catch (Exception ex)
            {
                await jobFacContext.JobService.UpdateExitMessage(RunStatus.Failed, options.ExceptionExitCode, ex.ToString()).ConfigureAwait(false);
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }
    }
}
