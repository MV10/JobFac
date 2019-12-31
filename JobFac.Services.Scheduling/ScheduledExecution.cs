using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

namespace JobFac.Services.Scheduling
{
    public class ScheduledExecution : Grain, IScheduledExecution
    {
        private readonly ILogger logger;
        private readonly ScheduleRepository scheduleRepository;

        public ScheduledExecution(
            ILoggerFactory loggerFactory,
            ScheduleRepository scheduleRepo)
        {
            logger = loggerFactory.CreateLogger(ConstLogging.JobFacCategorySchedulerService);
            scheduleRepository = scheduleRepo;
        }

        private IJobFactory jobFactory;
        private bool executionRequested = false;

        public override async Task OnActivateAsync()
        {
            jobFactory = GrainFactory.GetGrain<IJobFactory>();
            await base.OnActivateAsync();
        }

        public async Task StartScheduledJob(PendingScheduledJob assignment)
        {
            if (executionRequested) return;
            executionRequested = true;

            var activation = await scheduleRepository.GetJobActivationValue(assignment.DefinitionId, assignment.ScheduleTarget);
            if (activation.HasContent()) return;

            string result = string.Empty;
            try
            {
                var options = new FactoryStartOptions { DefinitionId = assignment.DefinitionId };
                result = await jobFactory.StartJob(options);
            }
            catch(Exception ex)
            {
                result = $"exception {ex}";
            }
            finally
            {
                await scheduleRepository.UpdatedActivationValue(assignment.DefinitionId, assignment.ScheduleTarget, result);
            }
        }
    }
}
