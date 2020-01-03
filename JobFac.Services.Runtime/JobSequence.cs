using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFac.Services.Runtime
{
    public class JobSequence : Grain, IJobSequence
    {
        private string jobInstanceKey = null;
        private JobStatus<StatusSequence> status = null;
        private JobDefinition<DefinitionSequence> jobDefinition = null;
        private IReadOnlyList<DefinitionSequenceStep> steps = null;
        private DefinitionSequenceStep currentStep = null;
        private IJobFactory jobFactory = null;

        private readonly DefinitionsRepository definitionRepo;
        private readonly HistoryRepository historyRepo;

        public JobSequence(
            DefinitionsRepository definitions,
            HistoryRepository history)
        {
            definitionRepo = definitions;
            historyRepo = history;
        }

        public override async Task OnActivateAsync()
        {
            jobInstanceKey = this.GetPrimaryKeyString();
            var history = await historyRepo.GetJobHistory(jobInstanceKey);
            if (history != null) status = historyRepo.DeserializeDetails<StatusSequence>(history);
            jobFactory = GrainFactory.GetGrain<IJobFactory>();
        }

        public async Task Start(JobDefinition<DefinitionSequence> jobDefinition, FactoryStartOptions options)
        {
            if (status != null)
                throw new Exception($"Sequence has already been started (instance {jobInstanceKey})");

            jobDefinition.ThrowIfInvalid();

            steps = await definitionRepo.GetStepsForSequence(jobDefinition.Id);

            if (steps.Count == 0)
                throw new Exception($"Unable to retrieve steps for sequence {jobDefinition.Id}");

            this.jobDefinition = jobDefinition;

            status = new JobStatus<StatusSequence>
            {
                Key = jobInstanceKey,
                StartOptions = options,
                RunStatus = RunStatus.StartRequested,
                LastUpdated = DateTimeOffset.UtcNow,
            };
            await historyRepo.InsertStatus(status);

            await StartNextStep();
        }

        public Task<JobStatus<StatusSequence>> GetStatus()
        {
            if (status == null)
                throw new Exception($"Job has not been started (instance {jobInstanceKey})");

            return Task.FromResult(status);
        }

        public async Task SequencedJobStatusChanged(JobStatus<StatusExternalProcess> jobStatus)
        {
            if (status == null)
                throw new Exception($"Sequence has not been started (instance {jobInstanceKey})");

            if(!status.JobTypeProperties.JobInstanceStepMap.ContainsKey(jobStatus.Key))
                throw new Exception($"Job {jobStatus.Key} has no status stored in sequence {jobInstanceKey}");

            var stepNum = status.JobTypeProperties.JobInstanceStepMap[jobStatus.Key];
            var stepStatus = status.JobTypeProperties.StepStatus[stepNum];

            if(!stepStatus.JobStatus.ContainsKey(jobStatus.Key))
                throw new Exception($"Job {jobStatus.Key} has no status stored for sequence {jobInstanceKey} step {stepNum}");

            stepStatus.JobStatus[jobStatus.Key] = jobStatus;

            // only examine exits related to the current step (jobs can be fire-and-forget from earier steps)
            if(jobStatus.HasExited && currentStep?.Step == stepNum)
            {
                bool evaluateExitDecision = (currentStep.ExitDecision == StepExitDecision.DoActionWhenAnyExit);
                if (currentStep.ExitDecision == StepExitDecision.DoActionWhenAllExit)
                    evaluateExitDecision = stepStatus.JobStatus.All(kvp => kvp.Value.HasExited);

                if(evaluateExitDecision)
                {
                    bool successes = stepStatus.JobStatus.Where(kvp => kvp.Value.HasExited).Any(kvp => !kvp.Value.HasFailed);
                    bool failures = stepStatus.JobStatus.Where(kvp => kvp.Value.HasExited).Any(kvp => kvp.Value.HasFailed);
                    bool mixed = successes && failures;

                    stepStatus.ExitResultSuccess = successes && !failures;
                    stepStatus.ExitResultMixed = mixed;

                    if (mixed) await DoExitAction(currentStep.ExitMixedResultsAction, currentStep.ExitMixedStepNumber);
                    if (!mixed && successes) await DoExitAction(currentStep.ExitSuccessAction, currentStep.ExitSuccessStepNumber);
                    if (!mixed && failures) await DoExitAction(currentStep.ExitFailureAction, currentStep.ExitFailureStepNumber);
                }
            }
            else
            {
                await historyRepo.UpdateStatus(status);
            }

            async Task DoExitAction(StepAction action, int stepNumber)
            {
                stepStatus.ExitResultStepNumber = stepNumber;

                switch(action)
                {
                    case StepAction.DoNextStep:
                        await historyRepo.UpdateStatus(status);
                        await StartNextStep();
                        break;

                    case StepAction.DoStepNumber:
                        await historyRepo.UpdateStatus(status);
                        await StartNextStep(stepNumber);
                        break;

                    case StepAction.EndSequence:
                        await StoreNewRunStatus(RunStatus.Ended);
                        break;
                }
            }
        }

        public async Task EndSequence()
        {
            if (status == null)
                throw new Exception($"Sequence has not been started (instance {jobInstanceKey})");

            currentStep = null;
            await StoreNewRunStatus(RunStatus.Ended);
        }

        public async Task Stop()
        {
            if (status == null)
                throw new Exception($"Sequence has not been started (instance {jobInstanceKey})");

            if (status.HasExited)
                throw new Exception($"Sequence has already exited (instance {jobInstanceKey})");

            if (status.RunStatus != RunStatus.Running)
                throw new Exception($"Sequence is not in Running status (instance {jobInstanceKey})");

            await StoreNewRunStatus(RunStatus.StopRequested);

            foreach(var jobKvp in CurrentStepStatus().JobStatus)
            {
                if(jobKvp.Value.RunStatus == RunStatus.Running)
                {
                    var job = GrainFactory.GetGrain<IJobExternalProcess>(jobKvp.Value.Key);
                    await job.Stop();
                }
            }
        }

        private async Task StartNextStep(int skipToStepNumber = 0)
        {
            var stepNum = (skipToStepNumber == 0) ? ++status.JobTypeProperties.SequenceStep : skipToStepNumber;
            currentStep = steps.Where(s => s.Step == stepNum).FirstOrDefault();

            // TODO set correct status and wrap up job processing before throwing
            if (currentStep == null)
                throw new Exception($"Step {stepNum} not defined for sequence {jobDefinition.Id}");
            
            var stepStatus = new StatusSequenceStep { Step = status.JobTypeProperties.SequenceStep };
            status.JobTypeProperties.StepStatus.Add(stepNum, stepStatus);

            if (!await ProcessStartDecision()) return;

            var jobIds = Formatting.SplitCommaSeparatedList(currentStep.JobDefinitionIdList);
            foreach(var id in jobIds)
            {
                var options = new FactoryStartOptions
                {
                    DefinitionId = id,
                    SequenceInstanceId = jobInstanceKey,
                    // TODO filter spawned-job args/payloads to the specific job?
                    ReplacementArguments = status.StartOptions.ReplacementArguments,
                    StartupPayloads = status.StartOptions.StartupPayloads
                };
                var jobId = await jobFactory.StartJob(options);
                status.JobTypeProperties.JobInstanceStepMap.Add(jobId, stepNum);
                stepStatus.JobStatus.Add(jobId, new JobStatus<StatusExternalProcess> 
                { 
                    Key = jobId, 
                    StartOptions = options, 
                    RunStatus = RunStatus.Unknown,
                });
            }

            await StoreNewRunStatus(RunStatus.Running);

            if (currentStep.ExitDecision == StepExitDecision.DoNextStepWithoutWaiting)
            {
                await StartNextStep();
            }

            // local function returns value indicating whether to continue start-up
            async Task<bool> ProcessStartDecision()
            {
                // The validator ensures StartDates and StartTimes are correct.
                if (currentStep.StartDateDecision != StepStartDateDecision.NoDecision)
                {
                    var analysis = new DateTimeAnalysis(currentStep.StartDecisionTimeZone);

                    var dates = Formatting.SplitCommaSeparatedList(currentStep.StartDates);
                    bool startDecision = currentStep.StartDateDecision switch
                    {
                        // see scheduler TargetPlanner for descriptions 
                        StepStartDateDecision.DaysOfWeek => analysis.InDaysOfWeek(dates),
                        StepStartDateDecision.DaysOfMonth => analysis.InDaysOfMonth(dates),
                        StepStartDateDecision.SpecificDates => analysis.InSpecificDates(dates),
                        StepStartDateDecision.DateRanges => analysis.InDateRanges(dates),
                        StepStartDateDecision.Weekdays => analysis.InWeekdays(dates),
                        _ => true,
                    };

                    if (startDecision && currentStep.StartTimeDecision != StepStartTimeDecision.NoDecision)
                    {
                        var times = Formatting.SplitCommaSeparatedList(currentStep.StartTimes);
                        startDecision = currentStep.StartTimeDecision switch
                        {
                            StepStartTimeDecision.IfHours => analysis.InHours(times),
                            StepStartTimeDecision.IfMinutes => analysis.InMinutes(times),
                            StepStartTimeDecision.IfTime => analysis.InSpecificTimes(times),
                            StepStartTimeDecision.IfTimeRange => analysis.InTimeRanges(times),
                            _ => true,
                        };
                    }

                    stepStatus.StartDecisionSuccess = startDecision;

                    if (!startDecision)
                    {
                        await historyRepo.UpdateStatus(status);
                        switch (currentStep.StartFalseAction)
                        {
                            case StepAction.DoNextStep:
                                await StartNextStep();
                                break;

                            case StepAction.DoStepNumber:
                                await StartNextStep(currentStep.StartFalseStepNumber);
                                break;

                            case StepAction.EndSequence:
                                await Stop();
                                break;
                        }
                        return false;
                    }
                }
                return true;
            }
        }

        private async Task StoreNewRunStatus(RunStatus runStatus)
        {
            // TODO notifications

            var now = DateTimeOffset.UtcNow;
            status.LastUpdated = now;
            status.RunStatus = runStatus;
            switch (runStatus)
            {
                case RunStatus.StartRequested:
                    status.StartRequested = now;
                    break;

                case RunStatus.StartFailed:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Running:
                    status.HasStarted = true;
                    break;

                case RunStatus.Stopped:
                    status.HasFailed = true;
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Ended:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                case RunStatus.Failed:
                    status.HasFailed = true;
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;

                // not possible to end up here?
                case RunStatus.Unknown:
                    status.HasExited = true;
                    status.ExitStateReceived = now;
                    break;
            }

            await historyRepo.UpdateStatus(status);
        }

        private StatusSequenceStep CurrentStepStatus()
            => status.JobTypeProperties.StepStatus[currentStep.Step];
    }
}
