using JobFac.Library.DataModels;
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
            try
            {
                Console.WriteLine("Querying repository.");
                var jobDef = await definitionRepo.GetJobDefinition<DefinitionExternalProcess>("Sample.JobFac.unaware");

                Console.WriteLine($"Response is null? {jobDef is null}");
                if (jobDef is null) return;

                Console.WriteLine($"ExecutablePathname:\n{jobDef.JobTypeProperties.ExecutablePathname}");
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
