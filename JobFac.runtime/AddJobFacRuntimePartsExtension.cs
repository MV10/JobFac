﻿using JobFac.runtime;

namespace Orleans.Hosting
{
    public static class AddJobFacRuntimePartsExtension
    {
        public static ISiloBuilder AddJobFacRuntimeParts(this ISiloBuilder builder)
        {
            builder.ConfigureApplicationParts(parts =>
            {
                parts.AddApplicationPart(typeof(JobFactory).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(Job).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(Sequence).Assembly).WithReferences();
                parts.AddApplicationPart(typeof(DataUtilities).Assembly).WithReferences();
                //parts.AddApplicationPart(typeof(SchedulerSingleton).Assembly).WithReferences();
            });
            return builder;
        }
    }
}
