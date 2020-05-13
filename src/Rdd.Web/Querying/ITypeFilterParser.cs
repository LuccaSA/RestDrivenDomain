using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface ITypeFilterParser<TEntity>
    {
        TypeFilter<TEntity> Parse(HttpRequest request);
    }
}