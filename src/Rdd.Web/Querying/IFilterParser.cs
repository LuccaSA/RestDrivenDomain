using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IFilterParser
    {
        WebFilter<TEntity> Parse<TEntity>(string key, string value);
        Filter<TEntity> Parse<TEntity>(HttpRequest request, IWebFilterConverter<TEntity> webFilterConverter) where TEntity : class;
    }
}