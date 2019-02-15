using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Domain.Models
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

        public Task<bool> AnyAsync(Query<TEntity> query) => Repository.AnyAsync(query);

        public virtual async Task<ISelection<TEntity>> GetAsync(Query<TEntity> query)
        {
            var count = 0;
            IEnumerable<TEntity> items = new HashSet<TEntity>();

            //Dans de rares cas on veut seulement le count des entités
            if (query.Options.NeedsCount && !query.Options.NeedsEnumeration)
            {
                count = await Repository.CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (query.Options.NeedsEnumeration)
            {
                items = await Repository.GetAsync(query);

                count = items.Count();

                //Si y'a plus d'items que le paging max ou que l'offset du paging n'est pas à 0, il faut compter la totalité des entités
                if (query.Page.Offset > 0 || query.Page.Limit <= count)
                {
                    count = await Repository.CountAsync(query);
                }

                items = await Repository.PrepareAsync(items, query);
            }

            return new Selection<TEntity>(items, count);
        }

        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            return (await GetAsync(new Query<TEntity>(query, e => e.Id.Equals(id)))).Items.FirstOrDefault();
        }

        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());
    }
}