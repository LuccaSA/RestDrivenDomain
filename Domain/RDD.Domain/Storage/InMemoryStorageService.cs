using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Storage
{
	public class InMemoryStorageService : IStorageService, IDisposable
	{
		protected Queue<Task> _afterAfterSaveChangesActions { get; set; }
		public Dictionary<Type, IList> Cache { get; private set; }
		public Dictionary<Type, int> Indexes { get; private set; }

		public InMemoryStorageService()
		{
			_afterAfterSaveChangesActions = new Queue<Task>();
			Cache = new Dictionary<Type, IList>();
			Indexes = new Dictionary<Type, int>();
		}

		private void CreateIfNotExist<TEntity>()
		{
			var type = typeof(TEntity);

			if (!Cache.ContainsKey(type))
			{
				Cache.Add(type, new List<TEntity>());
				Indexes.Add(type, 0);
			}
		}

		public IQueryable<TEntity> Includes<TEntity>(IQueryable<TEntity> entities, PropertySelector<TEntity> includes)
			where TEntity : class
		{
			return entities;
		}

		public IQueryable<TEntity> Set<TEntity>()
			where TEntity : class, IEntityBase
		{
			CreateIfNotExist<TEntity>();

			var type = typeof(TEntity);

			if (type.IsAbstract)
			{
				//If abstract, aggregate all subclasses
				//Does not handle recursivity (if subType is also abstract, it won't look into subsubtypes !)
				var subTypes = Cache.Keys.Where(k => type.IsAssignableFrom(k));

				return subTypes.SelectMany(t => Cache[t].Cast<TEntity>()).AsQueryable();
			}

			return Cache[typeof(TEntity)].Cast<TEntity>().AsQueryable();
		}

		public void Add<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			CreateIfNotExist<TEntity>();

			Cache[typeof(TEntity)].Add((object)entity);
		}

		public void Remove<TEntity>(TEntity entity)
			where TEntity : class, IEntityBase
		{
			CreateIfNotExist<TEntity>();
			Cache[typeof(TEntity)].Remove((object)entity);
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IEntityBase
		{
			foreach (var entity in entities)
			{
				Add<TEntity>(entity);
			}
		}

		public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IEntityBase
		{
			foreach (var entity in entities)
			{
				Remove<TEntity>(entity);
			}
		}

		public void AddAfterSaveChangesAction(Task action)
		{
			_afterAfterSaveChangesActions.Enqueue(action);
		}

		public async Task SaveChangesAsync()
		{
			foreach(var type in Cache.Keys)
			{
				var index = Indexes[type];

				foreach(var element in Cache[type])
				{
					var entity = (IPrimaryKey)element;
					var id = entity.GetId().ToString();

					if (id == 0.ToString())
					{
						entity.SetId(++index);
					}
				}

				Indexes[type] = index;
			}

			while(_afterAfterSaveChangesActions.Count() > 0)
			{
				await _afterAfterSaveChangesActions.Dequeue();
			}
		}
		public void Dispose() { }
	}
}
