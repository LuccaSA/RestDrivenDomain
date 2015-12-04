﻿using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IStorageService : IDisposable
	{
		IQueryable<TEntity> Set<TEntity>()
			where TEntity : class;

		IQueryable<TEntity> Includes<TEntity>(IQueryable<TEntity> entities, PropertySelector<TEntity> includes)
			where TEntity : class;

		void Add<TEntity>(TEntity entity)
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