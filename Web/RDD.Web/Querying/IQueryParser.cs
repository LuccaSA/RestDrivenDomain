using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public interface IQueryParser<TEntity> where TEntity : class
    {
        void IgnoreFilters(params string[] filters);
        Query<TEntity> Parse(HttpContext context, bool isCollectionCall);
        Query<TEntity> Parse(HttpVerbs verb, IEnumerable<KeyValuePair<string, StringValues>> parameters, bool isCollectionCall);
    }
}