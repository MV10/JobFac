using JobFac.lib.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.services
{
    // StatelessWorker grain
    public interface IDataUtilities : IGrainWithIntegerKey
    {
        Task WriteCapturedOutput(string instanceKey, string stdOut, string stdErr);
    }
}
