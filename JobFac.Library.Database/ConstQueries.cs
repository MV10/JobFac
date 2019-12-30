
namespace JobFac.Library.Database
{
    public static class ConstQueries
    {
        // DEFINITIONS REPOSITORY ////////////////////////////////////////////////

        public static readonly string SelectJobType = "SELECT JobType FROM JobDefinition WHERE Id = @Id;";
        public static readonly string SelectSequenceSteps = "SELECT * FROM StepDefinition WHERE SequenceId = @Id ORDER BY Step;";
        public static readonly string SelectJobDefinitionBase = "SELECT * FROM JobDefinition WHERE Id = @Id;";
        public static readonly string SelectSequenceDefinition = SelectJobDefinitionBase; // no additional properties
        public static readonly string SelectExternalProcessDefinition = "SELECT * FROM JobDefinition j INNER JOIN JobExternalProcess x ON x.Id = j.Id WHERE j.Id = @Id;";

        // HISTORY REPOSITORY ////////////////////////////////////////////////////

        public static readonly string SelectJobHistory = "SELECT * FROM JobHistory WHERE InstanceKey = @InstanceKey;";
        public static readonly string SelectJobHistoryAfter = "SELECT * FROM JobHistory WHERE DefinitionId = @DefinitionId AND LastUpdated >= @FromDate;";
        public static readonly string SelectJobHistoryBetween = "SELECT * FROM JobHistory WHERE DefinitionId = @DefinitionId AND LastUpdated BETWEEN @FirstDate AND @LastDate;";
        public static readonly string SelectJobHistoryActive = "SELECT InstanceKey FROM JobHistory WHERE DefinitionId = @DefinitionId AND FinalRunStatus = 0;";

        public static readonly string PurgeHistory = @"
        DELETE CapturedOutput FROM CapturedOutput c 
        INNER JOIN JobHistory h ON h.InstanceKey = c.InstanceKey AND h.DeleteAfter < @Now;
        DELETE FROM JobHistory WHERE DeleteAfter < @Now;";

        public static readonly string InsertJobHistory = @"INSERT INTO JobHistory 
        (InstanceKey, DefinitionId, LastUpdated, DeleteAfter, FinalRunStatus, FullDetailsJson)
        VALUES (@InstanceKey, @DefinitionId, @LastUpdated, @DeleteAfter, @FinalRunStatus, @FullDetailsJson);";

        public static readonly string UpdateJobHistory = @"UPDATE JobHistory SET
        DefinitionId = @DefinitionId, LastUpdated = @LastUpdated, DeleteAfter = @DeleteAfter, 
        FinalRunStatus = @FinalRunStatus, FullDetailsJson = @FullDetailsJson
        WHERE InstanceKey = @InstanceKey;";

        public static readonly string InsertCapturedOutput = @"INSERT INTO CapturedOutput
        (InstanceKey, StdOut, StdErr)
        VALUES (@InstanceKey, @StdOut, @StdErr)";

        // SCHEDULE REPOSITORY ////////////////////////////////////////////////

        public static readonly string SelectPendingScheduledJobs = "SELECT * FROM ScheduledJobs WHERE Activation = '' AND ScheduleTarget <= @ScheduleTarget;";

        public static readonly string InsertScheduledJob = @"INSERT INTO ScheduledJobs (DefinitionId, ScheduleTarget, Activation) VALUES (@DefinitionId, @ScheduleTarget, '');";

        public static readonly string UpdateScheduledJobActivation = "UPDATE ScheduledJobs SET Activation = @Activation WHERE ScheduleTarget = @ScheduleTarget AND DefinitionId = @DefinitionId;";

        // CONFIG REPOSITORY //////////////////////////////////////////////////

        public static readonly string SelectConfig = "SELECT * FROM Config;";

        public static readonly string UpdateConfig = "UPDATE Config SET ConfigValue = @ConfigValue WHERE ConfigKey = @ConfigKey;";
    }
}
