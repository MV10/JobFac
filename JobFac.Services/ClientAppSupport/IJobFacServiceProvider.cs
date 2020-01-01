using System;

namespace JobFac.Services
{
    public interface IJobFacServiceProvider : IAsyncDisposable
    {
        IJobFactory GetJobFactory();
        IJobExternalProcess GetExternalProcessJob(string instanceId);
    }
}
