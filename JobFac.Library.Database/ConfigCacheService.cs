using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Library.Database
{
    public class ConfigCacheService
    {
        private readonly ConfigRepository configRepository;

        public ConfigCacheService(ConfigRepository configRepository)
        {
            this.configRepository = configRepository;
        }

        private IReadOnlyDictionary<string, string> cache;
        private DateTimeOffset nextRefresh = DateTimeOffset.MinValue;

        public async Task<string> GetValue(string key)
        {
            if (DateTimeOffset.Now >= nextRefresh)
            {
                cache = await configRepository.GetConfig().ConfigureAwait(false);
                nextRefresh = DateTimeOffset.Now.AddMinutes(ConstTimeouts.ConfigCacheRefreshMinutes);
            }
            return cache[key] ?? string.Empty;
        }
    }
}
