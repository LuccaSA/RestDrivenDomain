﻿using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Infra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Services
{
	public class InMemoryStorageService : IStorageService, IDisposable
	{
		public Dictionary<Type, IList> Cache { get; private set; }
		public Dictionary<Type, int> Indexes { get; private set; }

		public InMemoryStorageService()
		{
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
			where TEntity : class
		{
			CreateIfNotExist<TEntity>();

			return Cache[typeof(TEntity)].Cast<TEntity>().AsQueryable();
		}

		public TEntity Add<TEntity>(TEntity entity)
			where TEntity : class, IPrimaryKey
		{
			CreateIfNotExist<TEntity>();

			Cache[typeof(TEntity)].Add((object)entity);

			return entity;
		}

		public void Remove<TEntity>(TEntity entity)
			where TEntity : class
		{
			CreateIfNotExist<TEntity>();
			Cache[typeof(TEntity)].Remove((object)entity);
		}

		public void AddRange<TEntity>(IEnumerable<TEntity> entities)
			where TEntity : class, IPrimaryKey
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

		public void Commit()
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
		}
		public void Dispose() { }
	}
}
