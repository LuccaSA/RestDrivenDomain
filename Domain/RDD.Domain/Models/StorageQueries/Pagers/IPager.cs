using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Pagers
{
	public interface IPager
	{
		IQueryable<T> Page<T>(IQueryable<T> source);
	}
}