using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NExtends.Expressions;
using RDD.Domain.Helpers;

namespace RDD.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyRestCollection(IReadOnlyRepository<TEntity> repository)
        {
            ReadOnlyRepository = repository;
        }

        protected IReadOnlyRepository<TEntity> ReadOnlyRepository { get; }

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            query.NeedEnumeration = false;
            query.NeedCount = true;

            await GetAsync(query);
            return query.QueryMetadata.TotalCount > 0;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() => (await GetAsync(new Query<TEntity>()));

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query)
        {
            await OnBeforeGetAsync();

            var totalCount = -1;
            IReadOnlyCollection<TEntity> items = null;

            //Dans de rares cas on veut seulement le count des entités
            if (query.NeedCount && !query.NeedEnumeration)
            {
                query.QueryMetadata.TotalCount = totalCount = await ReadOnlyRepository.CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (query.NeedEnumeration)
            {
                items = await ReadOnlyRepository.GetAsync(query);
                query.QueryMetadata.TotalCount = totalCount != -1 ? totalCount : await ReadOnlyRepository.CountAsync(query);
                items = await ReadOnlyRepository.PrepareAsync(items, query);
            }
            await OnAfterGetAsync(items);
            return items ?? Enumerable.Empty<TEntity>();
        }
         
        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());

        /// <summary>
        /// Central place to filter or add on post query
        /// </summary>
        /// <param name="source">Original items</param>
        /// <returns>Altered items</returns>
        protected virtual Task<IEnumerable<TEntity>> OnAfterGetAsync(IEnumerable<TEntity> source) 
            => Task.FromResult(source);

        /// <summary>
        /// Called before any Get
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnBeforeGetAsync() => Task.CompletedTask;

        /// <summary>
        /// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
        /// puisque c'était explicitement cette entité qui était visée
        /// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query = null)
        {
            Query<TEntity> q;
            if (query == null)
            {
                q = new Query<TEntity>(e => e.Id.Equals(id));
            }
            else
            {
                q = query;
                q.Filter = new Filter<TEntity>(q.Filter.Expression.AndAlso(e => e.Id.Equals(id)));
            }
            var result = await GetAsync(q);
            return result.FirstOrDefault();
        }

        public Task<TEntity> GetByIdAsync(TKey id, HttpVerbs verb = HttpVerbs.Get)
            => GetByIdAsync(id, new Query<TEntity> { Verb = verb });
    }
}