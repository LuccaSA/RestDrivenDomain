using System;

namespace RDD.Domain.Exceptions
{ 
    public class MissingCombinationException : TechnicalException
    {
        public MissingCombinationException(Type entityType) 
            : base("Unreachable entity type { 0}. Consider adding Combinations to your Application." + entityType.Name)
        {
        }
    }
}
