using JobFac.lib.DataModels;
using JobFac.services;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JobFac.runtime
{
    public class Sequence : Grain, ISequence
    {
        public Sequence()
        {

        }

        public Task Start(SequenceDefinition sequenceDefinition, FactoryStartOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<SequenceStatus> GetStatus()
        {
            throw new NotImplementedException();
        }

        public Task UpdateJobStatus(JobStatus jobStatus)
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
