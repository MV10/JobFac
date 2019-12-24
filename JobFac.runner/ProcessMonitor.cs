using JobFac.lib.DataModels;
using JobFac.services;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.runner
{
    public static class ProcessMonitor
    {
        public static async Task RunJob(IJob jobService, JobDefinition jobDef, string jobKey)
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            StringBuilder capturedStdOut = new StringBuilder();
            StringBuilder capturedStdErr = new StringBuilder();

            var proc = new Process();
            proc.StartInfo.FileName = jobDef.ExecutablePathname;
            proc.StartInfo.WorkingDirectory = jobDef.WorkingDirectory;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            // for JobProc-aware jobs, first argument is the job instance key
            proc.StartInfo.Arguments = (jobDef.PrefixJobInstanceIdArgument) ? $"{jobKey} {jobDef.Arguments}" : jobDef.Arguments;

            // TODO stdout and stderr logging

            //if (jobDef.LogStdOut)
            //{
            //}

            //if(jobDef.LogStdErr)
            //{
            //}

            if (jobDef.CaptureStdOut)
            {
                proc.StartInfo.RedirectStandardOutput = true;
                proc.OutputDataReceived += (s, e) => { if (e?.Data != null) capturedStdOut.AppendLine(e.Data); };
            }

            if (jobDef.CaptureStdErr)
            {
                proc.StartInfo.RedirectStandardError = true;
                proc.ErrorDataReceived += (s, e) => { if (e?.Data != null) capturedStdErr.AppendLine(e.Data); };
            }

            try
            {
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

                // TODO stdout and stderr logging

                if (jobDef.CaptureStdOut) // || jobDef.LogStdOut)
                    proc.BeginOutputReadLine();

                if (jobDef.CaptureStdErr) // || jobDef.LogStdErr)
                    proc.BeginErrorReadLine();

                await jobService.UpdateRunStatus(RunStatus.Running);

                // TODO implement maximum run-time token cancellation

                await Task.WhenAny
                    (
                        WaitForExitAsync.Wait(proc, token),
                        KillCommand.MonitorNamedPipe(jobKey, token)
                    ).ConfigureAwait(false);

                if (!proc.HasExited)
                {
                    proc.Kill(true); // still fires process-exit event (WaitForExitAsync still running)
                    await jobService.UpdateExitMessage(RunStatus.Stopped, -1, "Stop command received, process killed");
                }
                else
                {
                    // TODO check for minimum run-time

                    // when true, job is JobProc-aware and should have called UpdateExitMessage
                    if (!jobDef.PrefixJobInstanceIdArgument)
                    {
                        var finalStatus = (proc.ExitCode < 0) ? RunStatus.Failed : RunStatus.Ended;
                        await jobService.UpdateExitMessage(finalStatus, proc.ExitCode, string.Empty);
                    }
                    // TODO play it safe and retrieve status and verify JobProc-aware job actually set an exit message?
                }

                tokenSource.Cancel();
            }
            finally
            {
                proc?.Close();
                proc?.Dispose();
                tokenSource?.Dispose();
            }

            if (jobDef.CaptureStdOut || jobDef.CaptureStdErr)
                await jobService.WriteCapturedOutput(jobKey, capturedStdOut.ToString(), capturedStdErr.ToString());
        }
    }
}
