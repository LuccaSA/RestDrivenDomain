using RDD.Domain.Models.Querying.Selectors.ExpressionSelectors;
using RDD.Domain.Models.StorageQueries.Includers;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public class SelectorsTreeRoot<TSubject> : SelectorsTree<IdentitySelector<TSubject>>, ISelectorTreeIncluder<TSubject>
	{
		IdentitySelector<TSubject> _node;
		public override IdentitySelector<TSubject> Node => _node;

		/// <summary>
		/// Utilisé par reflection !
		/// </summary>
		public SelectorsTreeRoot()
		{
			_node = new IdentitySelector<TSubject>();
		}

		public IIncluder<TSubject> GetIncluder() => new MultiIncluder<TSubject>(GetIncluders());

		public IReadOnlyCollection<IMonoIncluder<TSubject>> GetIncluders()
		{
			return _children.Cast<ISelectorTreeIncluder<TSubject>>().SelectMany(c => c.GetIncluders()).ToList();
		}
	}
}