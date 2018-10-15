using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using System.Collections.Generic;

namespace Rdd.Web.Querying
{
    public interface IQueryParser<TEntity> where TEntity : class
    {
        void IgnoreFilters(params string[] filters);
        Query<TEntity> Parse(HttpRequest request, bool isCollectionCall);
    }
}