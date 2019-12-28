using JobFac.Library.Constants;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace JobFac.Library.Database
{
    public abstract class DapperRepositoryBase
    {
        private protected string connectionString { get; set; }

        public DapperRepositoryBase(IConfiguration config)
        {
            connectionString = config.GetConnectionString(ConstConfigKeys.StorageConnectionStringName);
        }

        private protected async Task<T> QueryOneAsync<T>(string sql, object param)
            => (await QueryAsync<T>(sql, param).ConfigureAwait(false)).FirstOrDefault();

        private protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            var result = await conn.QueryAsync<T>(sql, param).ConfigureAwait(false);
            await conn.CloseAsync().ConfigureAwait(false);
            return result.ToList();
        }

        private protected async Task ExecAsync(string sql, object param)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await conn.ExecuteAsync(sql, param).ConfigureAwait(false);
            await conn.CloseAsync().ConfigureAwait(false);
        }

    }
}
