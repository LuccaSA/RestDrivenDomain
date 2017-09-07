using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using System.Linq;

namespace RDD.Domain.Models.Rights
{
	class RightService<T> : ReadRightService<T>, IRightService<T>
	{
		public RightService(IExecutionContext execution, ICombinationsHolder combinationsHolder) : base(execution, combinationsHolder) { }

		public bool HasCreationRight(T input) => HasRight(input, HttpVerb.POST);
		public bool HasDeletionRight(T input) => HasRight(input, HttpVerb.DELETE);
		public bool HasEditionRight(T input) => HasRight(input, HttpVerb.PUT);

		protected virtual bool HasRight(T input, HttpVerb verb)
		{
			var operations = GetOperationIds(verb);
			return operations.Any() && _execution.curPrincipal.HasAnyOperations(operations);
		}

		public void ThrowIfNoCreationRight(T input) => ThrowIfNoRight(input, HttpVerb.POST);
		public void ThrowIfNoDeletionRight(T input) => ThrowIfNoRight(input, HttpVerb.DELETE);
		public void ThrowIfNoEditionRight(T input) => ThrowIfNoRight(input, HttpVerb.PUT);

		protected virtual void ThrowIfNoRight(T input, HttpVerb verb)
		{
			var operations = GetOperationIds(verb);
			if (!operations.Any())
			{
				throw new UnreachableEntityTypeException<T>();
			}
			if (!_execution.curPrincipal.HasAnyOperations(operations))
			{
				throw new UnauthorizedException(string.Format("You do not have sufficient permission to make a {0} on type {1}", verb, typeof(T).Name));
			}
		}
	}
}