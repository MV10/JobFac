using JobFac.Library.Database;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestDatabaseRepository : CoordinatedBackgroundService
    {
        private readonly DefinitionsRepository definitionRepo;

        public TestDatabaseRepository(
            IHostApplicationLifetime appLifetime,
            DefinitionsRepository definitionRepo
            )
            : base(appLifetime)
        {
            this.definitionRepo = definitionRepo;
        }

        protected override async Task ExecuteAsync()
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
