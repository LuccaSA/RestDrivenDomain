using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Orderers
{
	public interface IOrderer<T>
	{
		IOrderedQueryable<T> Order(IOrderedQueryable<T> source);
		IOrderedQueryable<T> Order(IQueryable<T> source);
	}
}