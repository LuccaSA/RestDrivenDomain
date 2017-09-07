using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Filters
{
	public interface IFilter<T>
	{
		IQueryable<T> ApplyFilter(IQueryable<T> source);
	}
}