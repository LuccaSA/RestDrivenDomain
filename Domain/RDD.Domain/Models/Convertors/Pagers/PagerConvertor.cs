using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Pagers;

namespace RDD.Domain.Models.Convertors.Pagers
{
	public class PagerConvertor<T> : IPagerConvertor<T> where T : class
	{
		public IPager ConverterToPager(Query<T> request)
		{
			if (request.Options.withPagingInfo)
			{
				return new Pager(request.Options.Page.Offset, request.Options.Page.Limit);
			}
			else
			{
				return new EmptyPager();
			}
		}
	}
}