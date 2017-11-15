using System;
using System.Net;

namespace RDD.Domain.Exceptions
{
    public class OutOfRangeException : BadRequestException
    { 
        public OutOfRangeException(string message) : base(message)
        {
        }

        public OutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
