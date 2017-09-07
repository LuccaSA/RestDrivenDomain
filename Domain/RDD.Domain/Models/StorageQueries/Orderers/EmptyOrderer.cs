using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Orderers
{
	public class EmptyOrderer<T> : IOrderer<T>
	{
		public IOrderedQueryable<T> Order(IQueryable<T> source) => source.OrderBy(e => 0);
		public IOrderedQueryable<T> Order(IOrderedQueryable<T> source) => source;
	}
}