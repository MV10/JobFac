
## JobFac.Services.Scheduling

Manages job scheduling and scheduled execution. These are strictly internal and automatic; jobs and clients never interact with these directly. There are four services and two utility components:

* ScheduleQueue is a singleton that partitions work to SchedulerService
* SchedulerService is a per-silo GrainService that executes jobs
* ScheduledExecution offloads job execution and ensures only-once execution
* ScheduleWriter updates the schedule table with the next day's jobs

* TargetDateAnalysis checks a variety of timezone-based target-date features
* TargetPlanner generates schedule entries for a given job and target-date

Negative and positive test cases for all date/time schedule options:

```
Now:    2020-01-02T06:40:56 America/New_York (-05)
Target: 2020-01-03T06:40:56 America/New_York (-05)

Job time.Interval:
  Date Mode: DaysOfWeek = 1,2,3,4,5,6,7
  Time Mode: Interval = 30
  Schedules: 48
  2020-01-03T00:00:00 America/New_York (-05)
  2020-01-03T00:30:00 America/New_York (-05)
  2020-01-03T01:00:00 America/New_York (-05)
  (45 variations omitted)

Job time.HoursMinutes:
  Date Mode: DaysOfWeek = 1,2,3,4,5,6,7
  Time Mode: HoursMinutes = 1030,1345,1715
  Schedules: 3
  2020-01-03T10:30:00 America/New_York (-05)
  2020-01-03T13:45:00 America/New_York (-05)
  2020-01-03T17:15:00 America/New_York (-05)

Job time.Minutes:
  Date Mode: DaysOfWeek = 1,2,3,4,5,6,7
  Time Mode: Minutes = 20,30,40
  Schedules: 72
  2020-01-03T00:20:00 America/New_York (-05)
  2020-01-03T00:30:00 America/New_York (-05)
  2020-01-03T00:40:00 America/New_York (-05)
  (69 variations omitted)

Job date.DaysOfWeek.negative:
  Date Mode: DaysOfWeek = 6
  Time Mode: HoursMinutes = 1130
  Schedules: 0

Job date.DaysOfMonth.positive:
  Date Mode: DaysOfMonth = 3
  Time Mode: HoursMinutes = 1130
  Schedules: 1
  2020-01-03T11:30:00 America/New_York (-05)

Job date.DaysOfMonth.negative:
  Date Mode: DaysOfMonth = 4
  Time Mode: HoursMinutes = 1130
  Schedules: 0

Job date.SpecificDates.positive:
  Date Mode: SpecificDates = 01/03
  Time Mode: HoursMinutes = 1130
  Schedules: 1
  2020-01-03T11:30:00 America/New_York (-05)

Job date.SpecificDates.negative:
  Date Mode: SpecificDates = 01/04
  Time Mode: HoursMinutes = 1130
  Schedules: 0

Job date.DateRanges.positive:
  Date Mode: DateRanges = 06/15-06/14,01/01-12/31
  Time Mode: HoursMinutes = 1130
  Schedules: 1
  2020-01-03T11:30:00 America/New_York (-05)

Job date.DateRanges.negative:
  Date Mode: DateRanges = 11/03-12/03
  Time Mode: HoursMinutes = 1130
  Schedules: 0

Job date.WeekdaysOfMonth.positive:
  Date Mode: Weekdays = weekday
  Time Mode: HoursMinutes = 1130
  Schedules: 1
  2020-01-03T11:30:00 America/New_York (-05)

Job date.WeekdaysOfMonth.negative:
  Date Mode: Weekdays = weekend
  Time Mode: HoursMinutes = 1130
  Schedules: 0
Calling StopApplication
```
