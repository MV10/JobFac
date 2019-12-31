
-- Corresponds to sample program "Sample.JobFac.unaware"

INSERT INTO [dbo].[JobDefinition] 
([Id], 
[JobType], [Category], [Name], [Description], [IsStartDisabled], [StartOnDemand], [StartBySchedule], 
[AlreadyRunningAction], [AlreadyRunningNotificationTargetType], [AlreadyRunningNotificationTarget],
[ScheduleDateMode], [ScheduleTimeMode], [ScheduleDates], [ScheduleTimes], [ScheduleTimeZone],
[ExecutionNotificationTargetType], [SuccessNotificationTargetType], [FailureNotificationTargetType], 
[ExecutionNotificationTarget], [SuccessNotificationTarget], [FailureNotificationTarget]) 
VALUES 
(N'Sample.JobFac.unaware', 
1, N'', N'Sample.JobFac.unaware', N'A console program that can''t retrieve payloads from the Job service.', 0, 1, 1, 
0, 0, N'',
0, 0, N'', N'', N'America/New_York',
0, 0, 0, N'', N'', N'')
GO

INSERT INTO [dbo].[JobExternalProcess]
([Id], 
[ExecutablePathname], [WorkingDirectory], 
[Username], [Password], 
[StartInSequence], 
[Arguments], [AllowReplacementArguments], [IsJobFacAware], 
[CaptureStdOut], [CaptureStdErr], [StdOutPathname], [StdErrPathname], 
[ObserveMaximumRunTime], [MaximumRunSeconds], [StopLongRunningJob], [MaximumRunTimeNotificationTargetType], [MaximumRunTimeNotificationTarget], 
[RetryWhenFailed], [OnlyNotifyOnce], [AllowRetryInSequences], [MaximumRetryCount], [RetryDelaySeconds])
VALUES
(N'Sample.JobFac.unaware', 
N'C:\Source\JobFac\Samples\job.JobFac.unaware\bin\Debug\netcoreapp3.1\job.JobFac.unaware.exe', N'C:\Source\JobFac\Samples\job.JobFac.unaware\bin\Debug\netcoreapp3.1', 
N'', N'', 
1, 
N'45', 1, 0, 
1, 1, N'', N'',
0, 0, 0, 0, N'', 
0, 1, 0, 0, 0)
GO

-- Corresponds to sample program "Sample.JobFac.aware"

INSERT INTO [dbo].[JobDefinition] 
([Id], 
[JobType], [Category], [Name], [Description], [IsStartDisabled], [StartOnDemand], [StartBySchedule], 
[AlreadyRunningAction], [AlreadyRunningNotificationTargetType], [AlreadyRunningNotificationTarget],
[ScheduleDateMode], [ScheduleTimeMode], [ScheduleDates], [ScheduleTimes], [ScheduleTimeZone],
[ExecutionNotificationTargetType], [SuccessNotificationTargetType], [FailureNotificationTargetType], 
[ExecutionNotificationTarget], [SuccessNotificationTarget], [FailureNotificationTarget]) 
VALUES 
(N'Sample.JobFac.aware', 
1, N'', N'Sample.JobFac.aware', N'A console program that can communicate with JobFac and requires a startup payload.', 0, 1, 1, 
0, 0, N'',
0, 0, N'', N'', N'America/New_York',
0, 0, 0, N'', N'', N'')
GO

INSERT INTO [dbo].[JobExternalProcess]
([Id], 
[ExecutablePathname], [WorkingDirectory], 
[Username], [Password], 
[StartInSequence], 
[Arguments], [AllowReplacementArguments], [IsJobFacAware], 
[CaptureStdOut], [CaptureStdErr], [StdOutPathname], [StdErrPathname], 
[ObserveMaximumRunTime], [MaximumRunSeconds], [StopLongRunningJob], [MaximumRunTimeNotificationTargetType], [MaximumRunTimeNotificationTarget], 
[RetryWhenFailed], [OnlyNotifyOnce], [AllowRetryInSequences], [MaximumRetryCount], [RetryDelaySeconds])
VALUES
(N'Sample.JobFac.aware', 
N'C:\Source\JobFac\Samples\job.JobFac.aware\bin\Debug\netcoreapp3.1\job.JobFac.aware.exe', N'C:\Source\JobFac\Samples\job.JobFac.aware\bin\Debug\netcoreapp3.1', 
N'', N'', 
1, 
N'', 1, 1, 
1, 1, N'', N'',
0, 0, 0, 0, N'', 
0, 1, 0, 0, 0)
GO