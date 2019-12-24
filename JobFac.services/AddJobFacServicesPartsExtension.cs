using JobFac.services;

namespace Orleans
{
    public static class AddJobFacServicesPartsExtension
    {
        public static IClientBuilder AddJobFacServicesParts(this IClientBuilder builder)
        {
            // Only interfaces intended to be used by the client are listed, although
            // technically they're all accessible since they're in the same assembly.
            // In the future these could be separated into client and non-client assemblies.
            builder.ConfigureApplicationParts(parts =>
            {
                parts.AddApplicationPart(typeof(IJobFactory).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IJob).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(ISequence).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(IDataUtilities).Assembly).WithReferences();
            });
            return builder;
        }
    }
}
