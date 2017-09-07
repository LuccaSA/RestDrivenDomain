using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Pagers
{
	public class EmptyPager : IPager
	{
		public IQueryable<T> Page<T>(IQueryable<T> source) => source;
	}
}