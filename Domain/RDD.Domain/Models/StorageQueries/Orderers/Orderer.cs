using RDD.Domain.Models.Querying;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.StorageQueries.Orderers
{
	public class Orderer<T, TKey> : IOrderer<T>
	{
		Expression<Func<T, TKey>> _keySelector;
		SortDirection _sortDirection;

		IOrderer<T> _next;

		public Orderer(Expression<Func<T, TKey>> keySelector, SortDirection sortDirection, IOrderer<T> next)
		{
			_keySelector = keySelector;
			_sortDirection = sortDirection;
			_next = next;
		}

		public IOrderedQueryable<T> Order(IQueryable<T> source)
		{
			return _next.Order(_sortDirection == SortDirection.Descending ? source.OrderByDescending(_keySelector) : source.OrderBy(_keySelector));
		}

		public IOrderedQueryable<T> Order(IOrderedQueryable<T> source)
		{
			return _next.Order(_sortDirection == SortDirection.Descending ? source.ThenByDescending(_keySelector) : source.ThenByDescending(_keySelector));
		}
	}
}