using System;


namespace JobFac.Library
{
    public class JobFacInvalidDataException : Exception
    {
        public JobFacInvalidDataException()
        { }

        public JobFacInvalidDataException(string message)
            : base(message)
        { }

        public JobFacInvalidDataException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
