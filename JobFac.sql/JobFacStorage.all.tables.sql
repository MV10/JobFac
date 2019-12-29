
-- All tables use non-clustered primary keys

DROP TABLE [dbo].[JobDefinition];
DROP TABLE [dbo].[JobExternalProcess];
DROP TABLE [dbo].[JobHistory];
DROP TABLE [dbo].[StepDefinition];
DROP TABLE [dbo].[CapturedOutput];
GO

-- Uniqueness and all reads are by Id only
CREATE TABLE [dbo].[JobDefinition]
(
	[Id] NVARCHAR(128) NOT NULL, 
    [JobType] INT NOT NULL, 
    [Category] NVARCHAR(128) NOT NULL, 
    [Name] NVARCHAR(128) NOT NULL, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [IsStartDisabled] INT NOT NULL, 
    [StartOnDemand] INT NOT NULL, 
    [StartBySchedule] INT NOT NULL, 
    [AlreadyRunningAction] INT NOT NULL, 
    [AlreadyRunningNotificationTargetType] INT NOT NULL, 
    [AlreadyRunningNotificationTarget] NVARCHAR(1024) NOT NULL, 
    [ScheduleDateMode] INT NOT NULL, 
    [ScheduleTimeMode] INT NOT NULL, 
    [ScheduleDates] NVARCHAR(128) NOT NULL, 
    [ScheduleTimes] NVARCHAR(128) NOT NULL, 
    [ExecutionNotificationTargetType] INT NOT NULL, 
    [SuccessNotificationTargetType] INT NOT NULL, 
    [FailureNotificationTargetType] INT NOT NULL, 
    [ExecutionNotificationTarget] NVARCHAR(1024) NOT NULL, 
    [SuccessNotificationTarget] NVARCHAR(1024) NOT NULL, 
    [FailureNotificationTarget] NVARCHAR(1024) NOT NULL, 

)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_JobDefinition ON
[dbo].[JobDefinition] ([Id] ASC);
GO

-- Uniqueness and all reads are by Id only
CREATE TABLE [dbo].[JobExternalProcess]
(
	[Id] NVARCHAR(128) NOT NULL, 
    [ExecutablePathname] NVARCHAR(1024) NOT NULL, 
    [WorkingDirectory] NVARCHAR(1024) NOT NULL, 
    [Username] NVARCHAR(128) NOT NULL, 
    [Password] NVARCHAR(128) NOT NULL, 
    [StartInSequence] INT NOT NULL, 
    [Arguments] NVARCHAR(1024) NOT NULL, 
    [AllowReplacementArguments] INT NOT NULL, 
    [IsJobFacAware] INT NOT NULL, 
    [CaptureStdOut] INT NOT NULL, 
    [CaptureStdErr] INT NOT NULL, 
    [StdOutPathname] NVARCHAR(1024) NOT NULL, 
    [StdErrPathname] NVARCHAR(1024) NOT NULL, 
    [ObserveMaximumRunTime] INT NOT NULL, 
    [MaximumRunSeconds] INT NOT NULL, 
    [StopLongRunningJob] INT NOT NULL, 
    [MaximumRunTimeNotificationTargetType] INT NOT NULL, 
    [MaximumRunTimeNotificationTarget] NVARCHAR(1024) NOT NULL, 
    [RetryWhenFailed] INT NOT NULL, 
    [OnlyNotifyOnce] INT NOT NULL, 
    [AllowRetryInSequences] INT NOT NULL, 
    [MaximumRetryCount] INT NOT NULL, 
    [RetryDelaySeconds] INT NOT NULL 
)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_JobExternalProcess ON
[dbo].[JobExternalProcess] ([Id] ASC);
GO


-- Uniqueness and all reads are by InstanceKey
-- Also has query index on DefintionId + LastUpdated
CREATE TABLE [dbo].[JobHistory]
(
    [InstanceKey] NVARCHAR(128) NOT NULL, 
	[DefinitionId] NVARCHAR(128) NOT NULL, 
    [LastUpdated] DATETIMEOFFSET NOT NULL,
    [DeleteAfter] DATETIMEOFFSET NOT NULL,
    [FinalRunStatus] INT NOT NULL,
    [FullDetailsJson] NVARCHAR(MAX) NOT NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_JobHistory ON
[dbo].[JobHistory] ([InstanceKey] ASC);
GO

CREATE NONCLUSTERED INDEX IX_JobHistoryQuery ON
[dbo].[JobHistory] ([DefinitionId] ASC, [LastUpdated] ASC);
GO

CREATE NONCLUSTERED INDEX IX_JobHistoryActive ON
[dbo].[JobHistory] ([DefinitionId] ASC, [FinalRunStatus] ASC);
GO

-- Uniqueness and all reads are by SequenceId or SequenceId + Step
CREATE TABLE [dbo].[StepDefinition]
(
	[SequenceId] NVARCHAR(128) NOT NULL, 
    [Step] INT NOT NULL,
    [Name] NVARCHAR(128) NOT NULL, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [JobDefinitionIdList] NVARCHAR(128) NOT NULL, 
    [StartDecision1] INT NOT NULL,
    [StartDecision2] INT NOT NULL,
    [StartCriteria1] NVARCHAR(128) NOT NULL, 
    [StartCriteria2] NVARCHAR(128) NOT NULL, 
    [StartTrueAction] INT NOT NULL,
    [StartFalseAction] INT NOT NULL,
    [StartTrueStepNumber] INT NOT NULL,
    [StartFalseStepNumber] INT NOT NULL,
    [ExitDecision] INT NOT NULL,
    [ExitSuccessAction] INT NOT NULL,
    [ExitFailureAction] INT NOT NULL,
    [ExitMixedResultsAction] INT NOT NULL,
    [ExitSuccessStepNumber] INT NOT NULL,
    [ExitFailureStepNumber] INT NOT NULL,
    [ExitMixedStepNumber] INT NOT NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_StepDefinition ON
[dbo].[StepDefinition] ([SequenceId] ASC, [Step] ASC);
GO

-- Uniqueness and all reads are by InstanceKey
CREATE TABLE [dbo].[CapturedOutput]
(
    [InstanceKey] NVARCHAR(128) NOT NULL, 
    [StdOut] NVARCHAR(MAX) NOT NULL,
    [StdErr] NVARCHAR(MAX) NOT NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_CapturedOutput ON
[dbo].[CapturedOutput] ([InstanceKey] ASC);
GO
