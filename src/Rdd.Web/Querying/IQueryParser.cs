using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain;
using Rdd.Infra.Web.Models;
using System;

namespace Rdd.Web.Querying
{
    public interface IQueryParser<TEntity, TKey>
        where TEntity : class, IPrimaryKey<TKey>
        where TKey : IEquatable<TKey>
    {
        HttpQuery<TEntity, TKey> Parse(HttpRequest request, bool isCollectionCall);
        HttpQuery<TEntity, TKey> Parse(HttpRequest request, ActionDescriptor action, bool isCollectionCall);
    }
}