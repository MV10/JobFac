using JobFac.Services.Scheduling;

namespace Orleans.Hosting
{
    public static class AddJobFacSchedulingServicesExtensions
    {
        public static ISiloBuilder AddJobFacSchedulingServices(this ISiloBuilder builder)
        {
            builder

            .AddGrainService<SchedulerService>()

            .ConfigureApplicationParts(parts =>
            {
                parts.AddApplicationPart(typeof(ISchedulerQueue).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(ISchedulerService).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IScheduledExecution).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IScheduleWriter).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(SchedulerQueue).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(SchedulerService).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(ScheduledExecution).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(ScheduleWriter).Assembly).WithReferences();
            });
            return builder;
        }
    }
}
