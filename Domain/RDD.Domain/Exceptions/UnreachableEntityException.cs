using System;
using System.Runtime.Serialization;

namespace Rdd.Domain.Exceptions
{
    [Serializable]
    public sealed class UnreachableEntityException : BadRequestException
    {
        public UnreachableEntityException(Type entityType)
            : base($"Unreachable entity type {entityType.Name}. Consider adding Combinations to your Application.")
        {
        }

        private UnreachableEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}