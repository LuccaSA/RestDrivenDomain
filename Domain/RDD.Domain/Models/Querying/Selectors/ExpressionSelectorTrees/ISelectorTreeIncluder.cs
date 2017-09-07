using RDD.Domain.Models.StorageQueries.Includers;
using System.Collections.Generic;

namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public interface ISelectorTreeIncluder<T> : IWritableSelectorsTree
	{
		IReadOnlyCollection<IMonoIncluder<T>> GetIncluders();
	}
}