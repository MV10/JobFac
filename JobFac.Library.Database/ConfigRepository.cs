using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFac.Library.Database
{
    public class ConfigRepository : DapperRepositoryBase
    {
        public ConfigRepository(IConfiguration config)
            : base(config)
        { }

        public async Task<IReadOnlyDictionary<string, string>> GetConfig()
        {
            var rows = await QueryAsync<ConfigTable>(ConstQueries.SelectConfig, null).ConfigureAwait(false);
            return rows.ToDictionary(x => x.ConfigKey, x => x.ConfigValue);
        }

        public async Task<string> ReadConfigValue(string key)
            => await QueryScalarAsync<string>(ConstQueries.SelectConfigValue, new { ConfigKey = key }).ConfigureAwait(false);

        public async Task UpdateConfig(string key, string value)
            => await ExecAsync(ConstQueries.UpdateConfig, new { ConfigKey = key, ConfigValue = value }).ConfigureAwait(false);
    }
}
