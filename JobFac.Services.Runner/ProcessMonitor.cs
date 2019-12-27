using JobFac.Library;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services.Runner
{
    public class ProcessMonitor : IHostedService
    {
        private readonly ILogger<ProcessMonitor> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IJobFacServiceProvider jobFacServices;

        public ProcessMonitor(
            ILogger<ProcessMonitor> logger,
            IHostApplicationLifetime appLifetime,
            IJobFacServiceProvider jobFacServices)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.jobFacServices = jobFacServices;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Runner starting, job instance {Program.JobInstanceKey}");
            // since this is an event handler, the lambda's async void is acceptable
            appLifetime.ApplicationStarted.Register(async () => await ExecuteAsync());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        private async Task ExecuteAsync()
        {
            logger.LogInformation($"Execution starting");
            var jobService = jobFacServices.GetJob(Program.JobInstanceKey);
            if (jobService == null)
                throw new Exception($"Unable to connect to job service (instance {Program.JobInstanceKey}");

            try
            {
                await jobService.UpdateRunStatus(RunStatus.StartRequested);
                var jobDef = await jobService.GetDefinition();
                await RunJob(jobService, jobDef);
            }
            catch (Exception ex)
            {
                logger.LogError($"ExecuteAsync {ex}");
                await jobService.UpdateExitMessage(RunStatus.Unknown, -1, $"JobFac.Services.Runner exception {ex}");
            }

            logger.LogInformation($"Execution ending, calling StopApplication after log-flush delay");
            await Task.Delay(10000);
            appLifetime.StopApplication();
        }

        private async Task RunJob(IJob jobService, JobDefinition jobDef)
        {
            logger.LogInformation($"RunJob starting");
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            StringBuilder dbStdOut = new StringBuilder();
            StringBuilder dbStdErr = new StringBuilder();

            StreamWriter fileStdOut = null;
            StreamWriter fileStdErr = null;

            var proc = new Process();
            proc.StartInfo.FileName = jobDef.ExecutablePathname;
            proc.StartInfo.WorkingDirectory = jobDef.WorkingDirectory;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            // for JobProc-aware jobs, first argument is the job instance key
            proc.StartInfo.Arguments = (jobDef.IsJobFacAware) ? $"{Program.JobInstanceKey} {jobDef.Arguments}" : jobDef.Arguments;

            try
            {
                if (jobDef.CaptureStdOut != JobStreamHandling.None)
                {
                    proc.StartInfo.RedirectStandardOutput = true;

                    if(jobDef.CaptureStdOut == JobStreamHandling.Database)
                        proc.OutputDataReceived += (s, e) => { if (e?.Data != null) dbStdOut.AppendLine(e.Data); };

                    // TODO if(jobDef.CaptureStdOut == JobStreamHandling.Logger)
                    // logger.Log(...)

                    if(jobDef.CaptureStdOut.IsFileBased())
                    {
                        var pathname = jobDef.CaptureStdOut == JobStreamHandling.TimestampedFile
                            ? jobDef.StdOutPathname.Replace("*", Formatting.FilenameTimestampUtcNow)
                            : jobDef.StdOutPathname;
                        fileStdOut = new StreamWriter(pathname, jobDef.CaptureStdOut == JobStreamHandling.AppendFile);
                        proc.OutputDataReceived += (s, e) => { if (e?.Data != null) fileStdOut.WriteLineAsync(e.Data); };
                    }
                }

                if (jobDef.CaptureStdErr != JobStreamHandling.None)
                {
                    proc.StartInfo.RedirectStandardError = true;

                    if (jobDef.CaptureStdErr == JobStreamHandling.Database)
                        proc.ErrorDataReceived += (s, e) => { if (e?.Data != null) dbStdErr.AppendLine(e.Data); };

                    // TODO if(jobDef.CaptureStdErr == JobStreamHandling.Logger)
                    // logger.Log(...)

                    if (jobDef.CaptureStdErr.IsFileBased())
                    {
                        var pathname = jobDef.CaptureStdErr == JobStreamHandling.TimestampedFile
                            ? jobDef.StdErrPathname.Replace("*", Formatting.FilenameTimestampUtcNow)
                            : jobDef.StdErrPathname;
                        fileStdErr = new StreamWriter(pathname, jobDef.CaptureStdErr == JobStreamHandling.AppendFile);
                        proc.ErrorDataReceived += (s, e) => { if (e?.Data != null) fileStdErr.WriteLineAsync(e.Data); };
                    }
                }

                logger.LogInformation($"RunJob calling Process.Start");
                try
                {
                    // Although Start returns a boolean, it isn't useful to us. The documentation
                    // says it returns false if a process is reused, which could happen if Process
                    // was used with the Windows shell to launch a document handler such as Word.
                    // That isn't a supported use-case. However, it can throw exceptions when, for
                    // example, the provided ExecutablePathname is invalid.
                    proc.Start();
                }
                catch(Exception ex)
                {
                    await jobService.UpdateExitMessage(RunStatus.StartFailed, -1, $"Job failed to start, exception {ex}");
                    return;
                }

                // These must be as close to proc.Start as possible to minimize the possibility of lost output
                if (jobDef.CaptureStdOut != JobStreamHandling.None) proc.BeginOutputReadLine();
                if (jobDef.CaptureStdErr != JobStreamHandling.None) proc.BeginErrorReadLine();

                await jobService.UpdateRunStatus(RunStatus.Running);

                // TODO implement maximum run-time token cancellation

                logger.LogInformation($"RunJob awaiting process exit or kill command");
                await Task.WhenAny
                    (
                        WaitForProcessExitAsync(proc, token),
                        MonitorKillCommandNamedPipe(token)
                    ).ConfigureAwait(false);
                logger.LogInformation($"RunJob await exited, Process.HasExited? {proc.HasExited}");


                if (!proc.HasExited)
                {
                    proc.Kill(true); // still fires process-exit event (WaitForExitAsync still running)
                    await jobService.UpdateExitMessage(RunStatus.Stopped, -1, "Stop command received, process killed");
                }
                else
                {
                    // when true, job is JobProc-aware and should have called UpdateExitMessage
                    if (!jobDef.IsJobFacAware)
                    {
                        var finalStatus = (proc.ExitCode < 0) ? RunStatus.Failed : RunStatus.Ended;
                        await jobService.UpdateExitMessage(finalStatus, proc.ExitCode, string.Empty);
                    }
                    // TODO play it safe and retrieve status and verify JobProc-aware job actually set an exit message?
                }

                logger.LogInformation($"RunJob cancelling token");
                tokenSource.Cancel();

                // The Process WaitForExit call without a timeout value is the
                // only way to asynchronously wait for the streams to drain.
                if (jobDef.CaptureStdOut != JobStreamHandling.None || jobDef.CaptureStdErr != JobStreamHandling.None)
                    proc.WaitForExit();
            }
            catch(Exception ex)
            {
                logger.LogError($"RunJob caught exception {ex}");
            }
            finally
            {
                logger.LogInformation($"RunJob finalizing");

                tokenSource?.Cancel();
                proc?.Close();

                tokenSource?.Dispose();
                proc?.Dispose();

                fileStdOut?.Close();
                fileStdErr?.Close();

                fileStdOut?.Dispose();
                fileStdErr?.Dispose();
            }

            if (jobDef.CaptureStdOut == JobStreamHandling.Database || jobDef.CaptureStdErr == JobStreamHandling.Database)
                await jobService.WriteCapturedOutput(Program.JobInstanceKey, dbStdOut.ToString(), dbStdErr.ToString());

            logger.LogInformation($"RunJob exiting");
        }

        private async Task WaitForProcessExitAsync(Process proc, CancellationToken token = default)
        {
            logger.LogInformation($"WaitForExit starting");
            var completion = new TaskCompletionSource<bool>();
            proc.EnableRaisingEvents = true;
            proc.Exited += CompleteTaskOnExit;
            try
            {
                if (proc.HasExited) return;
                using var reg = token.Register(() => Task.Run(() => completion.SetCanceled()));
                await completion.Task;
            }
            finally
            {
                proc.Exited -= CompleteTaskOnExit;
            }
            logger.LogInformation($"WaitForExit exiting");

            void CompleteTaskOnExit(object s, EventArgs e)
                => Task.Run(() => completion.TrySetResult(true));
        }

        // If this task exits, that means the Job service connected to this named pipe to request a process-kill
        private async Task MonitorKillCommandNamedPipe(CancellationToken token)
        {
            logger.LogInformation($"KillCommand starting");
            NamedPipeServerStream server = null;
            try
            {
                server = new NamedPipeServerStream(Program.JobInstanceKey, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(token);
                server.Disconnect();
            }
            finally
            {
                server?.Dispose();
            }
            logger.LogInformation($"KillCommand exiting");
        }
    }
}
