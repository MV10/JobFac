using System;

namespace JobFac.Library
{
    public class JobFacStartJobDeniedException : Exception
    {
        public JobFacStartJobDeniedException()
        { }

        public JobFacStartJobDeniedException(string message)
            : base(message)
        { }

        public JobFacStartJobDeniedException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
