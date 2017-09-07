using RDD.Domain.Models.StorageQueries;
using System.Collections.Generic;

namespace RDD.Domain.Contracts
{
	public interface IReadableRepository<T> where T : class
	{
		int Count(IStorageQuery<T> query);
		bool HasAny(IStorageQuery<T> query);
		IEnumerable<T> Get(IStorageQuery<T> query);
	}

	public interface IRepository<T> : IReadableRepository<T> where T : class
	{
		T Add(T input);
		void AddRange(IEnumerable<T> input);

		T Update(T input);

		void Delete(T input);
		void DeleteRange(IEnumerable<T> input);
	}
}
