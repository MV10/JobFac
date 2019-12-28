using JobFac.Library.Constants;
using JobFac.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Library.Logging
{
    // The ProviderAlias is only visible where the provider is actually
    // executing which, for JobFac, is typically the Runner process. This
    // isn't available within the IDataUtilities grain where the message
    // is re-output to the silo host logger. However, IDataUtilities will
    // re-use the constant as the log message category to allow filtering.

    [ProviderAlias(ConstLogging.JobFacLoggerProviderName)]
    public class JobFacLoggerProvider : BatchingLoggerProvider
    {
        private readonly IDataUtilities dataUtil;

        public JobFacLoggerProvider(
            IOptionsMonitor<BatchingLoggerOptions> options,
            IClusterClient clusterClient)
            : base(options)
        {
            dataUtil = clusterClient.GetGrain<IDataUtilities>(0);
        }

        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken token)
        {
            foreach(var msg in messages)
            {
                if (token.IsCancellationRequested) break;
                await dataUtil.RemoteLogger(msg.Level, msg.Message);
            }
        }
    }
}
