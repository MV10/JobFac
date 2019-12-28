using JobFac.Library.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace JobFac.Library.Logging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggingBuilder AddJobFacRemoteLogger(this ILoggingBuilder builder)
        {
            // Allow all Log Levels to pass since filtering of remote messages
            // should be controlled at the remote logger, not on this end of things.
            // Note this means if a non-JobFac process (like a JobFac-aware job) wishes
            // to write to the silo host logger, it must specify a category with this
            // same prefix (or inject ILogger<T> with type T in the JobFac namespace).
            builder.AddFilter(ConstLogging.JobFacLogCategoryPrefix, LogLevel.Trace);

            builder.Services.AddSingleton<ILoggerProvider, JobFacLoggerProvider>();
            return builder;
        }

        public static ILoggingBuilder AddJobFacRemoteLogger(this ILoggingBuilder builder, Action<BatchingLoggerOptions> configure)
        {
            if (configure == null) 
                throw new ArgumentNullException(nameof(configure));

            builder.AddJobFacRemoteLogger();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
