using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.StorageQueries.Filters
{
	public class Filter<T> : IFilter<T>
	{
		Expression<Func<T, bool>> _filter;

		public Filter(Expression<Func<T, bool>> filter)
		{
			_filter = filter;
		}

		public IQueryable<T> ApplyFilter(IQueryable<T> source) => source.Where(_filter);
	}
}