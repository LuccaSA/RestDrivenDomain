using System;

namespace Rdd.Infra.Exceptions
{
    public class SqlUniqConstraintException : Exception
    {
        public SqlUniqConstraintException(string message)
            : base(message) { }
    }
}
