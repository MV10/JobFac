using JobFac.Library.Database;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestDatabaseRepository : IHostedService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly DefinitionsRepository definitionRepo;

        public TestDatabaseRepository(
            IHostApplicationLifetime appLifetime,
            DefinitionsRepository definitionRepo
            )
        {
            this.appLifetime = appLifetime;
            this.definitionRepo = definitionRepo;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // since this is an event handler, the lambda's async void is acceptable
            appLifetime.ApplicationStarted.Register(async () => await ExecuteAsync());
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        private async Task ExecuteAsync()
        {
            Console.WriteLine("Querying repository.");
            var jobDef = await definitionRepo.GetJobDefinition("Sample.JobFac.unaware");

            Console.WriteLine($"Response is null? {jobDef is null}");
            if (jobDef is null) return;

            Console.WriteLine($"ExecutablePathname:\n{jobDef.ExecutablePathname}");

            Console.WriteLine("Calling StopApplication");
            appLifetime.StopApplication();
        }
    }
}
