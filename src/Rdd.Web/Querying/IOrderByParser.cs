using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IOrderByParser<TEntity>
         where TEntity : class
    {
        List<OrderBy<TEntity>> Parse(HttpRequest request);
    }
}