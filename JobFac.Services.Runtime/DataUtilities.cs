using JobFac.Library.Database;
using JobFac.Library.DataModels;
using JobFac.Services;
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
        private readonly HistoryRepository historyRepo;

        public DataUtilities(HistoryRepository history)
        {
            historyRepo = history;
        }

        public async Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr)
            => await historyRepo.InsertCapturedOutput(instanceKey, stdOut, stdErr);

    }
}
