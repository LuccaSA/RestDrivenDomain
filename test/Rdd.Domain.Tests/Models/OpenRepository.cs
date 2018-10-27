using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using System;
using System.Linq;

namespace Rdd.Domain.Tests.Models
{
    public class OpenRepository<TEntity, TKey> : Repository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        public OpenRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightsService)
        : base(storageService, rightsService) { }

        protected override IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, IQuery<TEntity> query)
        {
            if (query.Verb == Helpers.HttpVerbs.Get)
            {
                return entities;
            }

            return base.ApplyRights(entities, query);
        }
    }
}