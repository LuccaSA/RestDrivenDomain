using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IOrderByParser
    {
        List<OrderBy<TEntity>> Parse<TEntity>(HttpRequest request) where TEntity : class;
    }
}