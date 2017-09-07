using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Filters
{
	public class EmptyFilter<T> : IFilter<T>
	{
		public IQueryable<T> ApplyFilter(IQueryable<T> source) => source;
	}
}