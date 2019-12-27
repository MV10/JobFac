using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace JobFac.Library.Logging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggingBuilder AddJobFacRemoteLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, JobFacLoggerProvider>();
            return builder;
        }

        public static ILoggingBuilder AddJobFacRemoteLogger(this ILoggingBuilder builder, Action<BatchingLoggerOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            builder.AddJobFacRemoteLogger();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
