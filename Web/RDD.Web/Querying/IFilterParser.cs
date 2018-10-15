using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IFilterParser
    {
        WebFilter<TEntity> Parse<TEntity>(string key, string value);
    }
}