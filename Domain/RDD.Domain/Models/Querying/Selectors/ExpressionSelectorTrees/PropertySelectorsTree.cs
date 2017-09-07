using RDD.Domain.Models.Querying.Selectors.ExpressionSelectors;
using System.Reflection;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public class PropertySelectorsTree<TSubject, TProperty> : SelectorsTree<PropertySelector<TSubject, TProperty>>
		where TSubject : class
	{
		protected PropertySelector<TSubject, TProperty> _node;
		public override PropertySelector<TSubject, TProperty> Node => _node;

		/// <summary>
		/// Utilisé par reflection !
		/// </summary>
		/// <param name="property"></param>
		public PropertySelectorsTree(PropertyInfo property) : this(new PropertySelector<TSubject, TProperty>(property)) { }
		public PropertySelectorsTree(PropertySelector<TSubject, TProperty> node)
		{
			_node = node;
		}
	}
}