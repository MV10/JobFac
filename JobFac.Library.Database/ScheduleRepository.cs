using JobFac.Library.DataModels;
using JobFac.Library.DataModels.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFac.Library.Database
{
    public class ScheduleRepository : DapperRepositoryBase
    {
        public ScheduleRepository(IConfiguration config)
            : base(config)
        { }
    }
}
