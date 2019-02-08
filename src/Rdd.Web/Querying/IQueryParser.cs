using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain.Models.Querying;
using System;
using System.Linq.Expressions;

namespace Rdd.Web.Querying
{
    public interface IQueryParser<TEntity> where TEntity : class
    {
        Query<TEntity> Parse(HttpRequest request, bool isCollectionCall);
        Query<TEntity> Parse(HttpRequest request, ActionDescriptor action, bool isCollectionCall);
    }

    public interface ISubCollectionQueryParser<TEntity, TParentKey> where TEntity : class
    {
        Query<TEntity> Parse(HttpRequest request, TParentKey parentKey, Expression<Func<TEntity, TParentKey>> property, bool isCollectionCall);
        Query<TEntity> Parse(HttpRequest request, TParentKey parentKey, Expression<Func<TEntity, TParentKey>> property, ActionDescriptor action, bool isCollectionCall);
    }
}