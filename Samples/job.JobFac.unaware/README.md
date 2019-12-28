
## JobFac Sample: job.JobFac.unaware

Demonstrates a job which has no JobFac dependencies. It will be executed and monitored by the JobFac.Services.Runner application. It accepts one command-line argument indicating the number of seconds to sleep.

A run for 15 seconds produces this output:

```
Sample sleeping for 15 secs in 5 second increments.

Sleep interval #1
.....
Sleep interval #2
.....
Sleep interval #3
.....

Writing "OK" to stderr.
OK

Sample set exit code 12345
```

