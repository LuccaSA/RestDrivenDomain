using RDD.Domain.Models.Querying.Selectors.ExpressionSelectors;
using RDD.Domain.Models.StorageQueries.Includers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public class IncludableSelectorsTree<TSubject, TProperty> : PropertySelectorsTree<TSubject, TProperty>, ISelectorTreeIncluder<TSubject>
		where TSubject : class
		where TProperty : IIncludable
	{
		/// <summary>
		/// Utilisé par reflection !
		/// </summary>
		/// <param name="property"></param>
		public IncludableSelectorsTree(PropertyInfo property) : base(property) { }
		public IncludableSelectorsTree(PropertySelector<TSubject, TProperty> node) : base(node) { }

		public IReadOnlyCollection<IMonoIncluder<TSubject>> GetIncluders()
		{
			if (_children.Count == 0)
			{
				return new List<IMonoIncluder<TSubject>> { new Includer<TSubject, TProperty>(_node.Expression) };
			}
			else
			{
				return _children.Cast<ISelectorTreeIncluder<TProperty>>()
					.SelectMany(c => c.GetIncluders())
					.Select(i => new IncluderChain<TSubject, TProperty>(i, _node.Expression))
					.ToList();
			}
		}
	}
}