using System;

namespace JobFac.Library
{
    public class JobFacInvalidRunStatusException : Exception
    {
        public JobFacInvalidRunStatusException()
        { }

        public JobFacInvalidRunStatusException(string message)
            : base(message)
        { }

        public JobFacInvalidRunStatusException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
