using JobFac.database;
using JobFac.lib.DataModels;
using JobFac.services;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.runtime
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
