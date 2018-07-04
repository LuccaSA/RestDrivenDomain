using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Infra.Web.Models
{
    public class WebFiltersContainer<TEntity, TKey> : Filter<TEntity>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        private IEnumerable<WebFilter<TEntity>> _filters;

        public WebFiltersContainer(IEnumerable<WebFilter<TEntity>> filters)
        {
            Init(filters);
        }

        private void Init(IEnumerable<WebFilter<TEntity>> filters)
        {
            _filters = filters;

            Expression = new PredicateService<TEntity, TKey>(filters)
                .GetEntityPredicate(new QueryBuilder<TEntity, TKey>());
        }

        public bool HasFilter(Expression<Func<TEntity, object>> property)
        {
            return _filters.Any(f => f.Property.Contains(property));
        }

        public void RemoveFilter(Expression<Func<TEntity, object>> property)
        {
            if (HasFilter(property))
            {
                Init(_filters.Where(f => !f.Property.Contains(property)));
            }
        }

        public WebFilter<TEntity> GetFilter(Expression<Func<TEntity, object>> property)
        {
            return _filters.FirstOrDefault(f => f.Property.Contains(property));
        }
    }
}
