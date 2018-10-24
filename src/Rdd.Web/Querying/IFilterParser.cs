using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IFilterParser
    {
        Filter<TEntity> Parse<TEntity>(HttpRequest request, IWebFilterConverter<TEntity> webFilterConverter)
            where TEntity : class;

        Filter<TEntity> Parse<TEntity>(HttpRequest request, ActionDescriptor action, IWebFilterConverter<TEntity> webFilterConverter)
            where TEntity : class;

        WebFilter<TEntity> Parse<TEntity>(string key, string value);
    }
}