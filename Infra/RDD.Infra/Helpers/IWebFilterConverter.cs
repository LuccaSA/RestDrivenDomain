using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RDD.Infra.Web.Models;

namespace RDD.Infra.Helpers
{
    public interface IWebFilterConverter<TEntity>
    {
        Expression<Func<TEntity, bool>> ToExpression(IEnumerable<WebFilter<TEntity>> filters);
        Expression<Func<TEntity, bool>> ToExpression(WebFilter<TEntity> filter);
    }
}