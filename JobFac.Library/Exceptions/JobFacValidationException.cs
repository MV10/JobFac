using System;

namespace JobFac.Library
{
    public class JobFacValidationException : Exception
    {
        public JobFacValidationException()
        { }

        public JobFacValidationException(string message)
            : base(message)
        { }

        public JobFacValidationException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
