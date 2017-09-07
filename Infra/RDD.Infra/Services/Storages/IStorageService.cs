using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra.Services
{
	public interface IStorageService : IDisposable
	{
		IQueryable<TEntity> Set<TEntity>()
			where TEntity : class;

		TEntity Add<TEntity>(TEntity entity)
			where TEntity : class;

		void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class;

		void Remove<TEntity>(TEntity entity)
			where TEntity : class;

		void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class;

		void Commit();
	}
}