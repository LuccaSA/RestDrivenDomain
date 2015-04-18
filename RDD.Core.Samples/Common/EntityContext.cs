using RDD.Infra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.Common
{
	public class EntityContext : IStorageService, IDisposable
	{
		public Dictionary<Type, IList> Cache { get; private set; }

		public EntityContext()
		{
			Cache = new Dictionary<Type, IList>();
		}

		private void CreateIfNotExist<TEntity>()
		{
			var type = typeof(TEntity);

			if (!Cache.ContainsKey(type))
			{
				Cache.Add(type, new List<TEntity>());
			}
		}

		public IQueryable<TEntity> Includes<TEntity>(IQueryable<TEntity> entities, ICollection<string> includes)
			where TEntity : class
		{
			return entities;
		}

		public IQueryable<TEntity> Set<TEntity>()
			where TEntity : class
		{
			CreateIfNotExist<TEntity>();

			return Cache[typeof(TEntity)].Cast<TEntity>().AsQueryable();
		}

		public void Add<TEntity>(TEntity entity)
			where TEntity : class
		{
			CreateIfNotExist<TEntity>();

			Cache[typeof(TEntity)].Add((object)entity);
		}

		public void Remove<TEntity>(TEntity entity)
			where TEntity : class
		{
			CreateIfNotExist<TEntity>();
			Cache[typeof(TEntity)].Remove((object)entity);
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
		{
			foreach (var entity in entities)
			{
				Add<TEntity>(entity);
			}
		}

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class
		{
			foreach (var entity in entities)
			{
				Remove<TEntity>(entity);
			}
		}

		public void Commit() { }
		public void Dispose() { }
	}
}
