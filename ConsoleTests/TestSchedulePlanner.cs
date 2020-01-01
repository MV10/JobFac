using JobFac.Library.DataModels;
using JobFac.Services.Scheduling;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestSchedulePlanner : CoordinatedBackgroundService
    {
        public TestSchedulePlanner(IHostApplicationLifetime appLifetime)
            : base(appLifetime)
        {
        }

        protected override async Task ExecuteAsync()
        {
            try
            {
                var job = new DefinitionExternalProcess
                {

                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex}");
            }
            finally
            {
                Console.WriteLine("Calling StopApplication");
                appLifetime.StopApplication();
            }

        }
    }
}
