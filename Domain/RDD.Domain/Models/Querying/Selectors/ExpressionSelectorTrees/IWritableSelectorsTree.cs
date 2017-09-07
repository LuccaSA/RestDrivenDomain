
namespace RDD.Domain.Models.Querying.Selectors.ExpressionSelectorTrees
{
	public interface IWritableSelectorsTree : ISelectorsTree
	{
		void AddChild(ISelectorsTree child);
	}
}