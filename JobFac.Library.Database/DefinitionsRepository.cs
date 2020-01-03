using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO insert/update/delete jobs, steps, and query jobs by category (for admin tasks)

namespace JobFac.Library.Database
{
    public class DefinitionsRepository : DapperRepositoryBase
    {
        public DefinitionsRepository(IConfiguration config)
            : base(config)
        { }

        public async Task<JobDefinitionBase> GetJobDefinitionBase(string id)
            => await QueryOneRowAsync<JobDefinitionBase>(ConstQueries.SelectJobDefinitionBase, new { Id = id }).ConfigureAwait(false);

        public async Task<JobType> GetJobType(string id)
        {
            var code = await QueryScalarAsync<int>(ConstQueries.SelectJobType, new { Id = id }).ConfigureAwait(false);
            Enum.TryParse(code.ToString(), out JobType jobType);
            return jobType;
        }

        public async Task<JobDefinition<TJobTypeProperties>> GetJobDefinition<TJobTypeProperties>(string id)
        where TJobTypeProperties : class, new()
        {
            var jobType = await GetJobType(id).ConfigureAwait(false);

            if(!ConstTypeMaps.JobTypeGenericMap.ContainsKey(jobType) 
            || !ConstTypeMaps.JobTypeGenericMap[jobType].IsAssignableFrom(typeof(TJobTypeProperties)))
                return null;

            // https://github.com/StackExchange/Dapper#multi-mapping
            return await QueryMultiMapOneRowAsync<JobDefinition<TJobTypeProperties>, TJobTypeProperties, JobDefinition<TJobTypeProperties>> (
                ConstQueries.SelectExternalProcessDefinition, 
                (jobdef, props) => { jobdef.JobTypeProperties = props; return jobdef; },
                new { Id = id }).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<DefinitionSequenceStep>> GetStepsForSequence(string id)
            => await QueryAsync<DefinitionSequenceStep>(ConstQueries.SelectSequenceSteps, new { Id = id }).ConfigureAwait(false);
    }
}
