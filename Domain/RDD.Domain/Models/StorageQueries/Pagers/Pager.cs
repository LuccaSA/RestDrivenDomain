using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Pagers
{
	public class Pager : IPager
	{
		int _skipCount;
		int _takeCount;

		public Pager(int skipCount, int takeCount)
		{
			_skipCount = skipCount;
			_takeCount = takeCount;
		}

		public IQueryable<T> Page<T>(IQueryable<T> source)
		{
			return source.Skip(_skipCount).Take(_takeCount);
		}
	}
}