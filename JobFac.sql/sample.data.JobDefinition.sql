
-- A sample job, Id "Sample.JobFac.unaware"

INSERT INTO [dbo].[JobDefinition] 
([Id], [Category], [Name], [Description], [IsStartDisabled], [StartOnDemand], [StartBySchedule], [AllowOverlappingStartup], 
[ScheduleDateMode], [ScheduleTimeMode], [ScheduleDates], [ScheduleTimes], 
[ExecutionNotificationTargetType], [SuccessNotificationTargetType], [FailureNotificationTargetType], 
[ExecutionNotificationTarget], [SuccessNotificationTarget], [FailureNotificationTarget], 
[ExecutablePathname], [WorkingDirectory], 
[Username], [Password], 
[StartInSequence], 
[Arguments], [AllowReplacementArguments], [PrefixJobInstanceIdArgument], 
[CaptureStdOut], [CaptureStdErr], [StdOutPathname], [StdErrPathname], 
[RequireMinimumRunTime], [MinimumRunSeconds], [MinimumRunTimeNotificationTargetType], [MinimumRunTimeNotificationTarget], 
[ObserveMaximumRunTime], [MaximumRunSeconds], [StopLongRunningJob], [MaximumRunTimeNotificationTargetType], [MaximumRunTimeNotificationTarget], 
[RetryWhenFailed], [OnlyNotifyOnce], [AllowRetryInSequences], [MaximumRetryCount], [RetryDelaySeconds]) 
VALUES 
(N'Sample.JobFac.unaware', N'', N'Sample.JobFac.unaware', N'A console program that can''t retrieve payloads from the Job servce.', 0, 1, 1, 0, 
0, 0, N'', N'', 
0, 0, 0, N'', N'', N'', 
N'C:\Source\JobFac\Samples\job.JobFac.unaware\bin\Debug\netcoreapp3.1\job.JobFac.unaware.exe', N'C:\Source\JobFac\Samples\job.JobFac.unaware\bin\Debug\netcoreapp3.1', 
N'', N'', 
1, 
N'45', 1, 0, 
1, 1, N'', N'',
0, 0, 0, N'', 
0, 0, 0, 0, N'', 
0, 1, 0, 0, 0)

GO
