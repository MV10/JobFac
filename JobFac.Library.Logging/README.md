
## JobFac.Library.Logging

A specialized ILogger provider that sends log output to an Orleans grain where it is logged against the Orleans silo's logger instance. This allows the external job-Runner process to generate log output without actually being configured for whatever logging is in use by the silo.

In theory, a JobFac-aware job could also do this, although this isn't recommended and won't be documented.