using Rdd.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class InMemoryStorageService : IStorageService, IUnitOfWork
    {
        public Dictionary<Type, IList> Cache { get; }
        public Dictionary<Type, int> Indexes { get; }

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

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
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

        public virtual Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities)
            where TEntity : class
        {
            return Task.FromResult(entities.ToList() as IEnumerable<TEntity>);
        }

        public virtual Task<int> CountAsync<TEntity>(IQueryable<TEntity> entities)
             where TEntity : class
        {
            return Task.FromResult(entities.Count());
        }

        public void Add<TEntity>(TEntity entity)
            where TEntity : class
        {
            CreateIfNotExist<TEntity>();
            Cache[typeof(TEntity)].Add(entity);
        }

        public void Remove<TEntity>(TEntity entity)
            where TEntity : class
        {
            if (Cache.ContainsKey(typeof(TEntity)))
            {
                Cache[typeof(TEntity)].Remove(entity);
            }
        }

        public void DiscardChanges<TEntity>(TEntity entity) where TEntity : class { }

        public void AddRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public void Dispose() { }
    }
}