using JobFac.lib.Constants;
using JobFac.lib.DataModels;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

// https://github.com/StackExchange/Dapper#execute-a-query-and-map-the-results-to-a-strongly-typed-list

namespace JobFac.database
{
    public class DefinitionsRepository : DapperRepositoryBase
    {
        public DefinitionsRepository(IConfiguration config)
            : base(config)
        {
        }

        public async Task<JobDefinition> GetJobDefinition(string id)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            var jobDef = conn.Query<JobDefinition>("SELECT * FROM JobDefinition WHERE Id = @Id;", new { Id = id }).FirstOrDefault();
            await conn.CloseAsync();
            return jobDef;
        }

        public async Task<SequenceDefinition> GetSequenceDefinition(string id)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            var seqDef = conn.Query<SequenceDefinition>("SELECT * FROM SequenceDefinition WHERE Id = @Id;", new { Id = id }).FirstOrDefault();
            await conn.CloseAsync();
            return seqDef;
        }

        public async Task<IReadOnlyList<StepDefinition>> GetStepsForSequence(string id)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            var steps = conn.Query<StepDefinition>("SELECT * FROM StepDefinition WHERE SequenceId = @Id ORDER BY Step;", new { Id = id }).ToList();
            await conn.CloseAsync();
            return steps;
        }

        // TODO query job and sequence definition grouped by category, and insert/update/delete logic
    }
}
