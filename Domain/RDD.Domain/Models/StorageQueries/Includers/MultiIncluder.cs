using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Includers
{
	public class MultiIncluder<T> : IIncluder<T>
	{
		IReadOnlyCollection<IIncluder<T>> _includers;

		public MultiIncluder(IReadOnlyCollection<IIncluder<T>> includers)
		{
			_includers = includers;
		}

		public IQueryable<T> ApplyInclude(IQueryable<T> query)
		{
			var result = query;
			foreach (var includer in _includers)
			{
				result = includer.ApplyInclude(result);
			}
			return result;
		}
	}
}