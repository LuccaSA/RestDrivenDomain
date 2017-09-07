using RDD.Domain.Models.Querying.Selectors.ExpressionSelectors;
using System.Collections.Generic;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public interface ISelectorsTree
	{
		IEnumerable<ISelectorsTree> Children { get; }
		IExpressionSelector Node { get; }
	}
}