using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Includers
{
	public interface IIncluder<T>
	{
		IQueryable<T> ApplyInclude(IQueryable<T> query);
	}
}