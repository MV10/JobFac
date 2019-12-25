using JobFac.Library.Database;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AddDatabaseServicesExtension
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection serviceBuilder)
            => serviceBuilder
                .AddTransient<DefinitionsRepository>()
                .AddTransient<HistoryRepository>();
    }
}
