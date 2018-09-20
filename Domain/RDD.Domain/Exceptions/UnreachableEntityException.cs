using System;

namespace RDD.Domain.Exceptions
{
    public class UnreachableEntityException : BadRequestException
    {
        public UnreachableEntityException(Type entityType)
            : base($"Unreachable entity type {entityType.Name}. Consider adding Combinations to your Application.")
        {
        }
    }
}