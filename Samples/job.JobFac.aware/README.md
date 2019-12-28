
## JobFac Sample: job.JobFac.aware

Demonstrates a job which is able to communicate status information back to the JobFac services and requires a startup payload from the caller that executes the job.

A payload can be any string value -- a JSON-serialized object, for example. The sample code requires a simple pair of comma-delimited values. The first value is the number of seconds to delay, and the second value is output to the console and also returned as the job's exit-message.

A run with the payload "35,Hello world!" produces this output:

```
Sample sleeping for 35 secs in 5 second increments.
Second startup payload value is: Hello world!

Sleep interval #1
.....
Sleep interval #2
.....
Sleep interval #3
.....
Sleep interval #4
.....
Sleep interval #5
.....
Sleep interval #6
.....
Sleep interval #7
.....

Writing "OK" to stderr.

Sample set exit code 12345
Writing final exit status and message to JobFac.
```


