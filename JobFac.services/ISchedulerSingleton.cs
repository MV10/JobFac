using JobFac.Library.DataModels;
using Orleans;
using System.Threading.Tasks;

namespace JobFac.Services
{
    // Key 0, this is a cluster-level singleton (NOT a per-silo StatelessWorker)
    // Driven by 1-minute-interval reminder
    // http://dotnet.github.io/orleans/Documentation/grains/timers_and_reminders.html#reminder-usage
    public interface ISchedulerSingleton : IGrainWithIntegerKey
    {
    }
}
