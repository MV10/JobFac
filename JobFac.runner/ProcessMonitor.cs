using JobFac.lib;
using JobFac.lib.DataModels;
using JobFac.services;
using System;
using System.Diagnostics;
using System.IO;
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

            FileStream filetreamStdOut = null;
            FileStream filestreamStdErr = null;

            var proc = new Process();
            proc.StartInfo.FileName = jobDef.ExecutablePathname;
            proc.StartInfo.WorkingDirectory = jobDef.WorkingDirectory;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            // for JobProc-aware jobs, first argument is the job instance key
            proc.StartInfo.Arguments = (jobDef.PrefixJobInstanceIdArgument) ? $"{jobKey} {jobDef.Arguments}" : jobDef.Arguments;

            // TODO stdout and stderr logging

            if (jobDef.CaptureStdOut != JobStreamHandling.None)
            {
                proc.StartInfo.RedirectStandardOutput = true;

                if(jobDef.CaptureStdOut == JobStreamHandling.Database)
                    proc.OutputDataReceived += (s, e) => { if (e?.Data != null) capturedStdOut.AppendLine(e.Data); };

                // TODO if(jobDef.CaptureStdOut == JobStreamHandling.Logger)
                // logger.Log(...)

                if(jobDef.CaptureStdOut.IsFileBased())
                {
                    var pathname = jobDef.StdOutPathname;
                    var timestamp = DateTimeOffset.UtcNow.ToString();
                    if (jobDef.CaptureStdOut.IsTimestampedFile()) pathname.Replace("*", Formatting.FilenameTimestampUtcNow);
                }
            }

            if (jobDef.CaptureStdErr != JobStreamHandling.None)
            {
                proc.StartInfo.RedirectStandardError = true;

                if (jobDef.CaptureStdErr == JobStreamHandling.Database)
                    proc.ErrorDataReceived += (s, e) => { if (e?.Data != null) capturedStdErr.AppendLine(e.Data); };

                // TODO if(jobDef.CaptureStdErr == JobStreamHandling.Logger)
                // logger.Log(...)

                if (jobDef.CaptureStdErr.IsFileBased())
                {

                }
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

                if (jobDef.CaptureStdOut != JobStreamHandling.None)
                    proc.BeginOutputReadLine();

                if (jobDef.CaptureStdErr != JobStreamHandling.None)
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

                if(jobDef.CaptureStdOut != JobStreamHandling.None || jobDef.CaptureStdErr != JobStreamHandling.None)
                {
                    var streamDrainTimeout = DateTimeOffset.Now.AddSeconds(30);
                    // TODO is it safe to check both streams if only one is captured?
                    while (DateTimeOffset.Now < streamDrainTimeout && (!proc.StandardOutput.EndOfStream || !proc.StandardError.EndOfStream))
                        Thread.Sleep(100);
                }
            }
            finally
            {
                proc?.Close();
                proc?.Dispose();
                tokenSource?.Dispose();
            }

            if (jobDef.CaptureStdOut == JobStreamHandling.Database || jobDef.CaptureStdErr == JobStreamHandling.Database)
                await jobService.WriteCapturedOutput(jobKey, capturedStdOut.ToString(), capturedStdErr.ToString());
        }
    }
}
