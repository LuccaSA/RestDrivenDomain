using System.Linq;
using Microsoft.EntityFrameworkCore;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;

namespace Rdd.Infra.Storage
{
    public sealed class IncludeApplicator : IIncludeApplicator
    {
        public IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> entities, Query<TEntity> query, IExpressionTree includeWhiteList)
            where TEntity : class
        {
            if (includeWhiteList == null || query.Fields == null)
            {
                return entities;
            }

            foreach (var prop in query.Fields.Intersection(includeWhiteList))
            {
                entities = entities.Include(prop.Name);
            }

            return entities;
        }
    }
}
