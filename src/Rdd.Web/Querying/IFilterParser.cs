using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IFilterParser<TEntity>
        where TEntity : class
    {
        Filter<TEntity> Parse(HttpRequest request);
        Filter<TEntity> Parse(HttpRequest request, ActionDescriptor action);

        WebFilter<TEntity> Parse(string key, string value);
    }
}