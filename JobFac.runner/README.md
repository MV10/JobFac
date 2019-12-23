
## JobFac.runner

Console program which runs jobs and reports progress back to the `Job` service during the startup process, as well as execution results for jobs which are not JobFac-aware.

A job which is JobFac-aware will receive the job instance key (a GUID) as the first command-line argument. It must use this to connect to the correct `Job` service to send status updates, and can optionally query for more detailed payload data if the job is executed on-demand. JobFac-aware jobs can also provide an exit message in addition to the usual integer exit code.
