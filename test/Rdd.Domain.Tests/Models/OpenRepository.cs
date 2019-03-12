using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Domain.Tests.Models
{
    public class OpenRepository<TEntity> : Repository<TEntity>
        where TEntity : class
    {
        public OpenRepository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightsService)
        : base(storageService, rightsService) { }

        protected override async Task<IQueryable<TEntity>> ApplyRightsAsync(IQueryable<TEntity> entities, Query<TEntity> query)
        {
            if (query.Verb == Helpers.HttpVerbs.Get)
            {
                return entities;
            }

            return await base.ApplyRightsAsync(entities, query);
        }
    }
}