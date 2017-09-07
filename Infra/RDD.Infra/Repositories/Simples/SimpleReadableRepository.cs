using RDD.Domain.Contracts;
using RDD.Domain.Models.Rights;
using RDD.Domain.Models.StorageQueries;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra.Repositories.Simples
{
	public class SimpleReadableRepository<T> : SimpleQueryableFactory<T>, IReadableRepository<T> where T : class
	{
		public SimpleReadableRepository(IStorageService storageService, IReadRightService<T> rightService) : base(storageService, rightService) { }

		public int Count(IStorageQuery<T> query) => EvaluateQuery(query, q => q.Count());
		public bool HasAny(IStorageQuery<T> query) => EvaluateQuery(query, q => q.Any());
		public IEnumerable<T> Get(IStorageQuery<T> query) => EvaluateQuery(query, q => q.AsEnumerable());

		protected TResult EvaluateQuery<TResult>(IStorageQuery<T> query, Func<IQueryable<T>, TResult> method)
		{
			var queryable = PrepareQuery(query);

			query.Watch.Start();
			var result = method(queryable);
			query.Watch.Stop();

			return result;
		}
	}
}