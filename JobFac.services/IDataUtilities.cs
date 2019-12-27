using Orleans;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JobFac.Services
{
    // StatelessWorker grain
    public interface IDataUtilities : IGrainWithIntegerKey
    {
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
        Task RemoteLogger(string message);
        Task RemoteLogger(LogLevel level, string message);
    }
}
