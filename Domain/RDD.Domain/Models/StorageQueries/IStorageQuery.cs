using RDD.Domain.Models.StorageQueries.Filters;
using RDD.Domain.Models.StorageQueries.Includers;
using RDD.Domain.Models.StorageQueries.Orderers;
using RDD.Domain.Models.StorageQueries.Pagers;
using System.Diagnostics;

namespace RDD.Domain.Models.StorageQueries
{
	public interface IStorageQuery<T> where T : class
	{
		IFilter<T> Filter { get; }
		IOrderer<T> Orderer { get; }
		IPager Pager { get; }
		IIncluder<T> Includer { get; }

		Stopwatch Watch { get; }
	}
}