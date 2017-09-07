using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RDD.Domain.Models.Querying
{
    public class ExpressionQuery<TEntity> : Query<TEntity>
		where TEntity : class, IEntityBase
	{
		public Expression<Func<TEntity, bool>> ExpressionFilters { get; set; }

		public ExpressionQuery(Expression<Func<TEntity, bool>> filters)
		{
			ExpressionFilters = filters;
		}

		public override Expression<Func<TEntity, bool>> FiltersAsExpression()
		{
			return ExpressionFilters;
		}
	}
}
