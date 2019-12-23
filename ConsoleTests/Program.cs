using JobFac.database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTests
{
    class Program
    {
        static IServiceProvider services;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Reading config and building service provider.");
            services = AddConfiguration()
                .AddDatabaseServices()
                .BuildServiceProvider();

            await TestGetJobDefinition();
        }

        static async Task TestGetJobDefinition()
        {
            var defRepo = services.GetRequiredService<DefinitionsRepository>();
            Console.WriteLine("Querying repository.");
            var jobDef = await defRepo.GetJobDefinition("Sample.JobFac.unaware");
            Console.WriteLine($"Response is null? {jobDef is null}");
            if (jobDef is null) return;

            Console.WriteLine($"ExecutablePathname:\n{jobDef.ExecutablePathname}");
        }

        static IServiceCollection AddConfiguration()
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true, true)
              .Build();

            var servicesBuilder = new ServiceCollection()
                .AddSingleton((IConfiguration)configuration);

            return servicesBuilder;
        }
    }
}
