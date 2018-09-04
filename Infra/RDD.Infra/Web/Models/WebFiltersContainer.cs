using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Infra.Web.Models
{
    public class WebFiltersContainer<TEntity, TKey> : Filter<TEntity>
        where TEntity : IPrimaryKey<TKey>
    {
        private IEnumerable<WebFilter<TEntity>> _filters;

        public WebFiltersContainer(IEnumerable<WebFilter<TEntity>> filters)
        {
            Init(filters);
        }

        private void Init(IEnumerable<WebFilter<TEntity>> filters)
        {
            _filters = filters;

            Expression = new PredicateService<TEntity, TKey>(filters).GetEntityPredicate(new QueryBuilder<TEntity, TKey>());
        }

        public bool HasFilter<TProp>(Expression<Func<TEntity, TProp>> property) => GetFilter(property) != null;

        public void RemoveFilter<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            if (HasFilter(property))
            {
                var chain = new ExpressionSelectorParser().ParseChain(property);
                Init(_filters.Where(f => !f.Selector.Contains(chain)));
            }
        }

        public WebFilter<TEntity> GetFilter<TProp>(Expression<Func<TEntity, TProp>> property)
        {
            var chain = new ExpressionSelectorParser().ParseChain(property);
            return _filters.FirstOrDefault(f => f.Selector.Contains(chain));
        }
    }
}
