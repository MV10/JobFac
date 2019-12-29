using JobFac.Library.DataModels;
using System;
using System.Collections.Generic;

namespace JobFac.Library
{
    public static class ConstTypeMaps
    {
        public static readonly IReadOnlyDictionary<JobType, Type> JobTypeGenericMap = new Dictionary<JobType, Type>
        {
            { JobType.ExternalProcess, typeof(DefinitionExternalProcess) },
            { JobType.Sequence, typeof(DefinitionSequence) }
        };
    }
}
