using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rdd.Infra.Web.Models;

namespace Rdd.Infra.Helpers
{
    public interface IWebFilterConverter<TEntity>
    {
        Expression<Func<TEntity, bool>> ToExpression(IEnumerable<WebFilter<TEntity>> filters);
        Expression<Func<TEntity, bool>> ToExpression(WebFilter<TEntity> filter);
    }
}