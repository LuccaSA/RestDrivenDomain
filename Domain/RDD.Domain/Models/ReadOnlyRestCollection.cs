using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyRestCollection(IReadOnlyRepository<TEntity> repository)
        {
            Repository = repository;
        }

        protected IReadOnlyRepository<TEntity> Repository { get; set; }

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            query.Options.NeedEnumeration = false;
            query.Options.NeedCount = true;

            return (await GetAsync(query)).Count > 0;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() => (await GetAsync(new Query<TEntity> { Page = Page.Unlimited })).Items;

        public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var count = 0;
            IEnumerable<TEntity> items = new HashSet<TEntity>();

            //Dans de rares cas on veut seulement le count des entités
            if (query.Options.NeedCount && !query.Options.NeedEnumeration)
            {
                count = await Repository.CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (query.Options.NeedEnumeration)
            {
                items = await Repository.GetAsync(query);

                count = items.Count();

                //Si y'a plus d'items que le paging max ou que l'offset du paging n'est pas à 0, il faut compter la totalité des entités
                if (query.Page.Offset > 0 || query.Page.Limit <= count)
                {
                    count = await Repository.CountAsync(query);
                }

                query.Page.TotalCount = count;

                items = await Repository.PrepareAsync(items, query);
            }

            //Si c'était un PUT/DELETE, on en profite pour affiner la réponse
            if (query.Verb != HttpVerbs.Get && count == 0)
            {
                throw new NotFoundException(string.Format("No item of type {0} matching URL criteria while trying a {1}", typeof(TEntity).Name, query.Verb));
            }

            return new Selection<TEntity>(items, count);
        }

        /// <summary>
        /// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
        /// puisque c'était explicitement cette entité qui était visée
        /// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            TEntity result = (await GetAsync(new Query<TEntity>(query, e => e.Id.Equals(id)))).Items.FirstOrDefault();

            if (result == null)
            {
                throw new NotFoundException(string.Format("Resource with ID {0} not found", id));
            }

            return result;
        }

        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());

        public async Task<TEntity> TryGetByIdAsync(object id)
        {
            try
            {
                return await GetByIdAsync((TKey)id);
            }
            catch
            {
                return null;
            }
        }

        public Task<TEntity> GetByIdAsync(TKey id, HttpVerbs verb = HttpVerbs.Get)
            => GetByIdAsync(id, new Query<TEntity> { Verb = verb });
    }
}