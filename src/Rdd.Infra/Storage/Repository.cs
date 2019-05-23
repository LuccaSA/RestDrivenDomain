using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Infra.Storage
{
    public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        public Repository(IStorageService storageService, IRightExpressionsHelper<TEntity> rightExpressionsHelper)
            : base(storageService, rightExpressionsHelper) { }

        public virtual async Task AddAsync(TEntity entity, Query<TEntity> query)
        {
            if (query.Options.ChecksRights && !(await RightExpressionsHelper.GetFilterAsync(query)).Compile()(entity))
            {
                throw new ForbiddenException("You do not have the rights to create the posted entity");
            }

            StorageService.Add(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, Query<TEntity> query)
        {
            if (query.Options.ChecksRights)
            {
                var rightFilter = (await RightExpressionsHelper.GetFilterAsync(query)).Compile();
                if (!entities.All(rightFilter))
                {
                    throw new ForbiddenException("You do not have the rights to create one of the posted entity");
                }
            }

            StorageService.AddRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            StorageService.Remove(entity);
        }

        public void DiscardChanges(TEntity entity)
        {
            StorageService.DiscardChanges(entity);
        }
    }
}
