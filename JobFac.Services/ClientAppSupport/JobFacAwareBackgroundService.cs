using JobFac.Library;
using JobFac.Library.DataModels;
using JobFac.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public abstract class JobFacAwareBackgroundService : CoordinatedBackgroundService
    {
        protected readonly IJobFacServiceProvider jobFacServiceProvider;
        protected readonly JobFacAwareBackgroundServiceOptions jobFacServiceOptions;
        protected readonly string jobInstanceId;

        public JobFacAwareBackgroundService(
            IHostApplicationLifetime hostApplicationLifetime,
            IJobFacServiceProvider jobFacServiceProvider,
            JobFacAwareBackgroundServiceOptions jobFacServiceOptions)
            : base(hostApplicationLifetime)
        {
            if (jobFacServiceOptions.CommandLineArgs.Length == 0)
                throw new ArgumentException("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            jobInstanceId = Formatting.FormattedInstanceKey(jobFacServiceOptions.CommandLineArgs[0]);
            if (!jobInstanceId.HasContent())
                throw new ArgumentException("JobFac-aware apps require one GUID argument reflecting the job instance-key");

            this.jobFacServiceProvider = jobFacServiceProvider;
            this.jobFacServiceOptions = jobFacServiceOptions;
        }

        private string payload = null;
        private IJobExternalProcess service = null;

        protected string jobStartupPayload
        {
            get
            {
                if (payload == null)
                    throw new JobFacInvalidRunStatusException("The startup payload is unavailable until ExecuteAsync has been called");

                return payload;
            }
        }

        protected IJobExternalProcess jobService
        {
            get
            {
                if (service == null)
                    throw new JobFacInvalidRunStatusException("The job service reference is unavailable until ExecuteAsync has been called");

                return service;
            }
        }

        protected override async Task InitializingAsync(CancellationToken cancelInitToken)
        {
            try
            {
                service = jobFacServiceProvider.GetExternalProcessJob(jobInstanceId);
                if (service == null)
                    throw new JobFacConnectivityException($"Unable to connect to job service (instance {jobInstanceId}");

                cancelInitToken.ThrowIfCancellationRequested();

                if (jobFacServiceOptions.RetrieveStartupPayload)
                {
                    var payload = await jobService.GetStartupPayload();
                    if (!payload.HasContent())
                    {
                        if (jobFacServiceOptions.FailJobOnMissingStartupPayload)
                            throw new ArgumentException("The JobFac-aware sample requires a startup payload");

                        payload = string.Empty;
                    }
                }
                else
                {
                    payload = string.Empty;
                }

                cancelInitToken.ThrowIfCancellationRequested();

                await base.InitializingAsync(cancelInitToken);
            }
            catch (Exception ex)
            {
                if (service != null)
                    await service.UpdateExitMessage(RunStatus.Failed, jobFacServiceOptions.ExceptionExitCode, ex.ToString());
                appLifetime.StopApplication();
            }
        }
    }
}
