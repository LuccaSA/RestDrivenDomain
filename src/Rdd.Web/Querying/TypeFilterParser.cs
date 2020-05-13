using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public class TypeFilterParser<TEntity> : ITypeFilterParser<TEntity>
    {
        public TypeFilter<TEntity> Parse(HttpRequest request) => null;
    }
}