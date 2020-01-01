using Orleans;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public interface IDataUtilities : IStatelessWorkerGrain
    {
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
        Task RemoteLogger(string message);
        Task RemoteLogger(LogLevel level, string message);
    }
}
