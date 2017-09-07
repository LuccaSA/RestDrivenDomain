using System;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectors
{
	public class IdentitySelector<TSubject> : ExpressionSelector<Expression<Func<TSubject, TSubject>>>
	{
		public override Expression<Func<TSubject, TSubject>> Expression => t => t;
	}
}