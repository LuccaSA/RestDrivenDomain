using System;

namespace RDD.Domain.Exceptions
{ 
    public class UnreachableCombinationException : BusinessException
    {
        public UnreachableCombinationException(Type entityType) 
            : base("Unreachable entity type { 0}. Consider adding Combinations to your Application." + entityType.Name)
        {
        }
    }
}
