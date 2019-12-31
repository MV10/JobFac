using JobFac.Library;
using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Services;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Services.Runtime
{
    [StatelessWorker]
    public class DataUtilities : Grain, IDataUtilities
    {
        private readonly ILogger logForRemote;
        private readonly HistoryRepository historyRepo;

        public DataUtilities(
            ILoggerFactory loggerFactory,
            HistoryRepository historyRepository)
        {
            logForRemote = loggerFactory.CreateLogger(ConstLogging.JobFacRemoteLoggerProviderName);
            historyRepo = historyRepository;
        }

        public async Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr)
            => await historyRepo.InsertCapturedOutput(instanceKey, stdOut, stdErr);

        public Task RemoteLogger(string message)
        {
            logForRemote.Log(LogLevel.Information, message);
            return Task.CompletedTask;
        }

        public Task RemoteLogger(LogLevel level, string message)
        {
            logForRemote.Log(level, message);
            return Task.CompletedTask;
        }
    }
}
