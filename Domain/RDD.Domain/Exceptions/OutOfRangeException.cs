using System;

namespace RDD.Domain.Exceptions
{
    public class OutOfRangeException : BusinessException
    { 
        public OutOfRangeException(string message) : base(message)
        {
        }

        public OutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
