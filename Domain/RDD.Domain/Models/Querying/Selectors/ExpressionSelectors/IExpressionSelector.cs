using System.Linq.Expressions;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectors
{
	public interface IExpressionSelector
	{
		Expression Expression { get; }
	}
}