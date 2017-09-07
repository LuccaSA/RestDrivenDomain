using RDD.Domain.Models.Querying.Selectors.ExpressionSelectors;
using System.Collections.Generic;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public abstract class SelectorsTree<TSelector> : IWritableSelectorsTree
		where TSelector : IExpressionSelector
	{
		protected List<ISelectorsTree> _children;
		public IEnumerable<ISelectorsTree> Children => _children;

		IExpressionSelector ISelectorsTree.Node => Node;
		public abstract TSelector Node { get; }

		public SelectorsTree()
		{
			_children = new List<ISelectorsTree>();
		}

		public void AddChild(ISelectorsTree child) => _children.Add(child);
	}
}