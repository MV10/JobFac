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
    [ProviderAlias("JobFacRemote")]
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
                await dataUtil.RemoteLogger(msg.Message);
            }
        }
    }
}
