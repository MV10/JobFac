using JobFac.Services;
using JobFac.Services.Runtime;

namespace Orleans.Hosting
{
    public static class AddJobFacRuntimeServicesExtension
    {
        public static ISiloBuilder AddJobFacRuntimeServices(this ISiloBuilder builder)
        {
            builder.ConfigureApplicationParts(parts =>
            {
                parts.AddApplicationPart(typeof(IJobFactory).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IJobExternalProcess).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IDataUtilities).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(JobFactory).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(JobExternalProcess).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(DataUtilities).Assembly).WithReferences();
            });
            return builder;
        }
    }
}
