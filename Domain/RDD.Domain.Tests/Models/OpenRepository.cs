using Rdd.Application;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using System.Linq;

namespace Rdd.Domain.Tests.Models
{
    public class OpenRepository<TEntity> : Repository<TEntity>
        where TEntity : class
    {
        public OpenRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightsService)
        : base(storageService, rightsService) { }

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