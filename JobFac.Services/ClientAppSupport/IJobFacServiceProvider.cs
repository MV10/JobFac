using System;

namespace JobFac.Services
{
    public interface IJobFacServiceProvider : IAsyncDisposable
    {
        IJobFactory GetJobFactory();
        IJob GetJob(string jobInstanceId);
        ISequence GetSequence(string sequenceInstanceId);
    }
}
