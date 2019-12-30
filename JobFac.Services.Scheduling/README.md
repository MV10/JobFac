
## JobFac.Services.Scheduling

Manages the ScheduledJobs table and scheduled job execution. Four components:

* ScheduleQueue is a singleton that partitions work to SchedulerService
* SchedulerService is a per-silo GrainService that executes jobs
* ScheduledExecution is used to offload job execution
* ScheduleWriter updates the schedule table with the next day's jobs
