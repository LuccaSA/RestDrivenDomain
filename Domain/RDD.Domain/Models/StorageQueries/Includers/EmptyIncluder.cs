using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Includers
{
	class EmptyIncluder<T> : IIncluder<T>
	{
		public IQueryable<T> ApplyInclude(IQueryable<T> query) => query;
	}
}