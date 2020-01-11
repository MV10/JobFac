using System.Threading;
using System.Threading.Tasks;

namespace JobFac.Services
{
    public abstract class JobFacAwareProcessBase
    {
        public virtual bool ValidateArguments(string[] args) => true;

        public virtual bool ValidateStartupPayload(string payload)=> true;

        public abstract Task ExecuteProcessingAsync(JobFacAwareProcessContext jobFacContext, CancellationToken appStoppingToken);
    }
}
