using JobFac.Library;
using JobFac.Library.DataModels;
using JobFac.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services.Runner
{
    public static class ProcessMonitor
    {
        public static async Task RunJob(IJob jobService, JobDefinition jobDef, string jobKey)
        {
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
            proc.StartInfo.Arguments = (jobDef.IsJobFacAware) ? $"{jobKey} {jobDef.Arguments}" : jobDef.Arguments;

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
                    // when true, job is JobProc-aware and should have called UpdateExitMessage
                    if (!jobDef.IsJobFacAware)
                    {
                        var finalStatus = (proc.ExitCode < 0) ? RunStatus.Failed : RunStatus.Ended;
                        await jobService.UpdateExitMessage(finalStatus, proc.ExitCode, string.Empty);
                    }
                    // TODO play it safe and retrieve status and verify JobProc-aware job actually set an exit message?
                }

                tokenSource.Cancel();

                // The Process WaitForExit call without a timeout value is the only way to
                // asynchronously wait for the streams to drain.
                if (jobDef.CaptureStdOut != JobStreamHandling.None || jobDef.CaptureStdErr != JobStreamHandling.None)
                    proc.WaitForExit();
            }
            catch(Exception ex)
            {
                // TODO log exceptions
            }
            finally
            {
                proc?.Close();
                proc?.Dispose();

                tokenSource?.Dispose();

                fileStdOut?.Close();
                fileStdErr?.Close();
                fileStdOut?.Dispose();
                fileStdErr?.Dispose();
            }

            if (jobDef.CaptureStdOut == JobStreamHandling.Database || jobDef.CaptureStdErr == JobStreamHandling.Database)
                await jobService.WriteCapturedOutput(jobKey, dbStdOut.ToString(), dbStdErr.ToString());
        }
    }
}
