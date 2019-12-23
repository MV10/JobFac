using JobFac.database;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection serviceBuilder)
            => serviceBuilder
                .AddTransient<DefinitionsRepository>()
                .AddTransient<HistoryRepository>();
    }
}
