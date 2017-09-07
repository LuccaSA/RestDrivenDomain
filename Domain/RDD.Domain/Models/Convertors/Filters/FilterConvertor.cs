using LinqKit;
using RDD.Domain.Models.Convertors.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Convertors.Filters
{
	class FilterConvertor<T> : IFilterConvertor<T> where T : class
	{
		IExpressionGenerator<T> _generator;

		public FilterConvertor(IExpressionGenerator<T> generator)
		{
			_generator = generator;
		}

		public IFilter<T> ConverterToFilter(Query<T> request)
		{
			if (request.Filters != null)
			{
				return new Filter<T>(GetPredicate(request.Filters));
			}

			return new EmptyFilter<T>();
		}

		private Expression<Func<T, bool>> GetPredicate( IEnumerable<Where> wheres)
		{
			var feed = PredicateBuilder.True<T>();

			foreach (var where in wheres)
			{
				feed = feed.And(ToExpression(where, where.Values).Expand());
			}

			return feed.Expand();
		}

		private Expression<Func<T, bool>> ToExpression(Where where, IList value)
		{
			switch (where.Type)
			{
				case WhereOperand.Equals: return _generator.Equals(where.Field, value);
				case WhereOperand.NotEqual: return _generator.NotEqual(where.Field, value);
				case WhereOperand.Starts: return _generator.Starts(where.Field, value);
				case WhereOperand.Like: return _generator.Like(where.Field, value);
				case WhereOperand.Between: return _generator.Between(where.Field, value);
				case WhereOperand.Since: return _generator.Since(where.Field, value);
				case WhereOperand.Until: return _generator.Until(where.Field, value);
				case WhereOperand.GreaterThan: return _generator.GreaterThan(where.Field, value);
				case WhereOperand.GreaterThanOrEqual: return _generator.GreaterThanOrEqual(where.Field, value);
				case WhereOperand.LessThan: return _generator.LessThan(where.Field, value);
				case WhereOperand.LessThanOrEqual: return _generator.LessThanOrEqual(where.Field, value);
			}

			throw new IndexOutOfRangeException(String.Format("Unhandled where condition type {0}", where.Type));
		}
	}
}