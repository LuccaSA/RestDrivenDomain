using Rdd.Infra.Rights;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System;
using System.Linq;

namespace Rdd.Domain.Tests.Models
{
    public class OpenRepository<TEntity, TKey> : Repository<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        public OpenRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightsService, HttpQuery<TEntity, TKey> httpQuery)
        : base(storageService, rightsService, httpQuery) { }

        protected override IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (query.Verb == Helpers.HttpVerbs.Get)
            {
                return entities;
            }

            return base.ApplyRights(entities, query);
        }
    }
}