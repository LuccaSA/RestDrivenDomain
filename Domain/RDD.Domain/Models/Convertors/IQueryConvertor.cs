using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries;
using System.Diagnostics;

namespace RDD.Domain.Models.Convertors
{
	public interface IQueryConvertor<T> where T : class
	{
		IStorageQuery<T> Convert(Query<T> request);
		IStorageQuery<T> Convert(Query<T> request, Stopwatch watch);
	}
}