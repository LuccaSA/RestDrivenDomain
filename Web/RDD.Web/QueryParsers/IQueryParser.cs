using RDD.Domain.Models.Querying;
using RDD.Web.Contexts;

namespace RDD.Web.QueryParsers
{
	public interface IQueryParser<T>
	{
		void IgnoreFilters(params string[] filters);

		Query<T> ParseWebContext(IWebContext webContext, bool isCollectionCall);
	}
}