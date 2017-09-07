using RDD.Domain.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Models.Rights
{
	class ReadRightService<T> : IReadRightService<T>
	{
		protected IExecutionContext _execution;
		ICombinationsHolder _combinationsHolder;

		public ReadRightService(IExecutionContext execution, ICombinationsHolder combinationsHolder)
		{
			_execution = execution;
			_combinationsHolder = combinationsHolder;
		}

		public IQueryable<T> ApplyFilter(IQueryable<T> source)
		{
			return _execution.curPrincipal.ApplyRights(source, GetOperationIds(HttpVerb.GET));
		}

		protected virtual HashSet<int> GetOperationIds(HttpVerb verb)
		{
			var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(T) && c.Verb == verb);
			return new HashSet<int>(combinations.Select(c => c.Operation.Id));
		}
	}
}