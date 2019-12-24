using System;

// TODO consider validating format for individual options (ex. each ScheduleDateMode vs ScheduleDates content)

namespace JobFac.lib.DataModels
{
    public static class DefinitionValidationExtensions
    {
        public static void ThrowIfInvalid(this JobDefinition def)
        {
            ((BaseDefinition)def).ThrowIfInvalid();

            if (!def.ExecutablePathname.HasContent())
                Throw("ExecutablePathname required");

            if (!def.WorkingDirectory.HasContent())
                Throw("WorkingDirectory required");

            if (def.Username.HasContent() && !def.Password.HasContent())
                Throw("Password is required when Username is specified");

            if (def.CaptureStdOut.IsFileBased() && !def.StdOutPathname.HasContent())
                Throw("StdOutPathname must be set for file-based CaptureStdOut settings");

            if (def.CaptureStdErr.IsFileBased() && !def.StdErrPathname.HasContent())
                Throw("StdErrPathname must be set for file-based CaptureStdErr settings");

            if (def.CaptureStdOut == JobStreamHandling.TimestampedFile && !def.StdOutPathname.Contains("*"))
                Throw("StdOutPathname must contain an asterisk for timestamped-file CaptureStdOut settings");

            if (def.CaptureStdErr == JobStreamHandling.TimestampedFile && !def.StdErrPathname.Contains("*"))
                Throw("StdErrPathname must contain an asterisk for timestamped-file CaptureStdErr settings");

            if (def.ObserveMaximumRunTime && def.MaximumRunSeconds < 1)
                Throw("ObserveMaximumRunTime requires MaximumRunSeconds of 1 second or greater");

            if (def.MaximumRunTimeNotificationTargetType != NotificationTargetType.None && !def.MaximumRunTimeNotificationTarget.HasContent())
                Throw("setting MaximumRunTimeNotificationTargetType requires a value in MaximumRunTimeNotificationTarget field");

            if (def.RetryWhenFailed && def.MaximumRetryCount < 1)
                Throw("RetryWhenFailed requires MaximumRetryCount of 1 or greater");
        }

        public static void ThrowIfInvalid(this SequenceDefinition def)
        {
            ((BaseDefinition)def).ThrowIfInvalid();
            // currently sequences don't have anything to validate
        }

        public static void ThrowIfInvalid(this StepDefinition def)
        {
            if (!def.JobDefinitionIdList.HasContent())
                Throw("JobDefinitionIdList required");

            if (def.StartDecision1 == StepStartDecision.NoDecision && def.StartDecision2 != StepStartDecision.NoDecision)
                Throw("StartDecision1 must be set before setting StartDecision2");

            if (def.StartDecision1 != StepStartDecision.NoDecision && !def.StartCriteria1.HasContent())
                Throw("setting StartDecision1 requires a value in StartCriteria1");

            if (def.StartDecision2 != StepStartDecision.NoDecision && !def.StartCriteria2.HasContent())
                Throw("setting StartDecision2 requires a value in StartCriteria2");

            if (def.StartDecision1 != StepStartDecision.NoDecision && def.StartTrueStepNumber < 1)
                Throw("StartTrueStepnumber must be 1 or greater when StartDecisions are set");

            if (def.StartDecision1 != StepStartDecision.NoDecision && def.StartTrueStepNumber < 1)
                Throw("StartTrueStepnumber must be 1 or greater when StartDecisions are set");

            if (def.StartDecision1 != StepStartDecision.NoDecision && def.StartFalseStepNumber < 1)
                Throw("StartFalseStepNumber must be 1 or greater when StartDecisions are set");

            if (def.StartDecision1 != StepStartDecision.NoDecision && def.StartTrueStepNumber == def.StartFalseStepNumber)
                Throw("StartDecisions are set but StartTrueStepNumber and StartFalseStepNumber are the same value");

            if (def.ExitDecision != StepExitDecision.DoNextStepWithoutWaiting && (def.ExitSuccessStepNumber < 1 || def.ExitFailureStepNumber < 1 || def.ExitMixedStepNumber < 1))
                Throw("Exit_StepNumbers must 1 or greater when ExitDecision is set");
        }

        public static void ThrowIfInvalid(this FactoryStartOptions opt)
        {
            if (!opt.DefinitionId.HasContent())
                Throw("DefinitionId is required");

            if (opt.NotificationScope != NotificationScope.None && opt.NotificationTargetType != NotificationTargetType.None && !opt.NotificationTarget.HasContent())
                Throw("setting NotificationTargetType requires a value in NotificationTarget field");
        }

        private static void ThrowIfInvalid(this BaseDefinition def)
        {
            if (def.ScheduleDateMode != ScheduleDateMode.None && !def.ScheduleDates.HasContent())
                Throw("setting ScheduleDateMode requires a value in ScheduleDates field");

            if (def.ScheduleTimeMode != ScheduleTimeMode.None && !def.ScheduleTimes.HasContent())
                Throw("setting ScheduleTimeMode requires a value in ScheduleTimes field");

            if (def.ExecutionNotificationTargetType != NotificationTargetType.None && !def.ExecutionNotificationTarget.HasContent())
                Throw("setting ExecutionNotificationTargetType requires a value in ExecutionNotificationTarget field");

            if (def.SuccessNotificationTargetType != NotificationTargetType.None && !def.SuccessNotificationTarget.HasContent())
                Throw("setting SuccessNotificationTargetType requires a value in SuccessNotificationTarget field");

            if (def.FailureNotificationTargetType != NotificationTargetType.None && !def.FailureNotificationTarget.HasContent())
                Throw("setting FailureNotificationTargetType requires a value in FailureNotificationTarget field");
        }

        private static void Throw(string message)
            => throw new Exception($"Invalid definition, {message}");
    }
}
