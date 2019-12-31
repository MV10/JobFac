using JobFac.Library.DataModels.Abstractions;
using System;

namespace JobFac.Library.DataModels
{
    public static class DefinitionValidationExtensions
    {
        private static void ThrowIfInvalid(this JobDefinitionBase def)
        {
            if (def.ScheduleDateMode != ScheduleDateMode.Unscheduled)
            {
                if (def.ScheduleTimeMode == ScheduleTimeMode.Unscheduled)
                    Throw("setting ScheduleDateMode requires a ScheduleTimeMode and a value in ScheduleTimes field");

                if (!def.ScheduleDates.HasContent() || !def.ScheduleTimes.HasContent())
                    Throw("setting ScheduleDateMode requires values in ScheduleDates and ScheduleTimes fields");

                // TODO validate ScheduleDateMode vs ScheduleDates content
                // TODO validate ScheduleTimeMode vs ScheduleTimes content
                // TODO validate ScheduleTimeZone vs Noda Time database
            }

            if (def.ExecutionNotificationTargetType != NotificationTargetType.None && !def.ExecutionNotificationTarget.HasContent())
                Throw("setting ExecutionNotificationTargetType requires a value in ExecutionNotificationTarget field");

            if (def.SuccessNotificationTargetType != NotificationTargetType.None && !def.SuccessNotificationTarget.HasContent())
                Throw("setting SuccessNotificationTargetType requires a value in SuccessNotificationTarget field");

            if (def.FailureNotificationTargetType != NotificationTargetType.None && !def.FailureNotificationTarget.HasContent())
                Throw("setting FailureNotificationTargetType requires a value in FailureNotificationTarget field");
        }

        public static void ThrowIfInvalid(this JobDefinition<DefinitionExternalProcess> def)
        {
            ((JobDefinitionBase)def).ThrowIfInvalid();

            var props = def.JobTypeProperties;

            if (!props.ExecutablePathname.HasContent())
                Throw("ExecutablePathname required");

            if (!props.WorkingDirectory.HasContent())
                Throw("WorkingDirectory required");

            if (props.Username.HasContent() && !props.Password.HasContent())
                Throw("Password is required when Username is specified");

            if (props.CaptureStdOut.IsFileBased() && !props.StdOutPathname.HasContent())
                Throw("StdOutPathname must be set for file-based CaptureStdOut settings");

            if (props.CaptureStdErr.IsFileBased() && !props.StdErrPathname.HasContent())
                Throw("StdErrPathname must be set for file-based CaptureStdErr settings");

            if (props.CaptureStdOut == JobStreamHandling.TimestampedFile && !props.StdOutPathname.Contains("*"))
                Throw("StdOutPathname must contain an asterisk for timestamped-file CaptureStdOut settings");

            if (props.CaptureStdErr == JobStreamHandling.TimestampedFile && !props.StdErrPathname.Contains("*"))
                Throw("StdErrPathname must contain an asterisk for timestamped-file CaptureStdErr settings");

            if (props.ObserveMaximumRunTime && props.MaximumRunSeconds < 1)
                Throw("ObserveMaximumRunTime requires MaximumRunSeconds of 1 second or greater");

            if (props.MaximumRunTimeNotificationTargetType != NotificationTargetType.None && !props.MaximumRunTimeNotificationTarget.HasContent())
                Throw("setting MaximumRunTimeNotificationTargetType requires a value in MaximumRunTimeNotificationTarget field");

            if (props.RetryWhenFailed && props.MaximumRetryCount < 1)
                Throw("RetryWhenFailed requires MaximumRetryCount of 1 or greater");
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

        private static void Throw(string message)
            => throw new Exception($"Invalid definition, {message}");
    }
}
