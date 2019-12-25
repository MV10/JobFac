using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    // StatelessWorker grain
    public interface IDataUtilities : IGrainWithIntegerKey
    {
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
    }
}
