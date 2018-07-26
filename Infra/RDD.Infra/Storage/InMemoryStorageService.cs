using RDD.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
    public class InMemoryStorageService : IStorageService
    {
        protected Queue<Task> AfterSaveChangesActions { get; }
        public Dictionary<Type, IList> Cache { get; }
        public Dictionary<Type, int> Indexes { get; }

        public InMemoryStorageService()
        {
            AfterSaveChangesActions = new Queue<Task>();
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

        public virtual Task<IReadOnlyCollection<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities)
            where TEntity : class
        {
            return Task.FromResult(entities.ToList() as IReadOnlyCollection<TEntity>);
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
            CreateIfNotExist<TEntity>();
            Cache[typeof(TEntity)].Remove(entity);
        }

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

        public void AddAfterSaveChangesAction(Task action)
        {
            AfterSaveChangesActions.Enqueue(action);
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

            while(AfterSaveChangesActions.Any())
            {
                await AfterSaveChangesActions.Dequeue();
            }
        }
        public void Dispose()
        {
            //Nothing to dispose here
        }
    }
}
