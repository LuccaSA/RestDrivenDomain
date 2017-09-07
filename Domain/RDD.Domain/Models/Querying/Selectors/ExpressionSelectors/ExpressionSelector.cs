using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectors
{
	public abstract class ExpressionSelector<TExpression> : IExpressionSelector
		where TExpression : Expression
	{
		Expression IExpressionSelector.Expression => Expression;
		public abstract TExpression Expression { get; }
	}
}