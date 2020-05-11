using System.Linq;
using Microsoft.EntityFrameworkCore;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Z.EntityFramework.Plus;

namespace Rdd.Z.EntityFramework
{
    public class ZEntityIncludeApplicator : IIncludeApplicator
    {
        static ZEntityIncludeApplicator()
        {
            QueryIncludeOptimizedManager.AllowIncludeSubPath = true;
        }

        public IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> entities, Query<TEntity> query, IExpressionTree includeWhiteList) where TEntity : class
        {
            if (includeWhiteList == null || query.Fields == null)
            {
                return entities;
            }

            if (query.GetOptimizeIncludes())
            {
                foreach (var prop in query.Fields.Intersection(includeWhiteList))
                {
                    entities = entities.IncludeOptimizedByPath(prop.Name);
                }
            }
            else
            {
                foreach (var prop in query.Fields.Intersection(includeWhiteList))
                {
                    entities = entities.Include(prop.Name);
                }
            }

            return entities;
        }
    }
}
