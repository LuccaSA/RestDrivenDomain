using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Models.StorageQueries.Includers
{
	public class Includer<T, TProperty> : IMonoIncluder<T>
		where T : class
		where TProperty : IIncludable
	{
		Expression<Func<T, TProperty>> _expression;

		public Includer(Expression<Func<T, TProperty>> expression)
		{
			_expression = expression ?? throw new ArgumentNullException(nameof(expression));
		}

		IQueryable<T> IIncluder<T>.ApplyInclude(IQueryable<T> query) => query.Include(_expression);

		IQueryable<TInitial> IMonoIncluder<T>.ApplyInclude<TInitial>(IIncludableQueryable<TInitial, T> query) => query.ThenInclude(_expression);
	}

	public class IncluderChain<T, TProperty> : IMonoIncluder<T>
		where T : class
		where TProperty : IIncludable
	{
		IMonoIncluder<TProperty> _subIncluder;
		Expression<Func<T, TProperty>> _expression;

		public IncluderChain(IMonoIncluder<TProperty> subIncluder, Expression<Func<T, TProperty>> expression)
		{
			_subIncluder = subIncluder ?? throw new ArgumentNullException(nameof(subIncluder));
			_expression = expression ?? throw new ArgumentNullException(nameof(expression));
		}

		IQueryable<T> IIncluder<T>.ApplyInclude(IQueryable<T> query) => _subIncluder.ApplyInclude(query.Include(_expression));

		IQueryable<TInitial> IMonoIncluder<T>.ApplyInclude<TInitial>(IIncludableQueryable<TInitial, T> query) => _subIncluder.ApplyInclude(query.ThenInclude(_expression));
	}
}