using System;

namespace JobFac.Library
{
    public class JobFacConnectivityException : Exception
    {
        public JobFacConnectivityException()
        { }

        public JobFacConnectivityException(string message)
            : base(message)
        { }

        public JobFacConnectivityException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
