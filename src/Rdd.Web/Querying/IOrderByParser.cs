using Microsoft.AspNetCore.Http;
using Rdd.Infra.Web.Models;
using System.Collections.Generic;

namespace Rdd.Web.Querying
{
    public interface IOrderByParser
    {
        List<OrderBy<TEntity>> Parse<TEntity>(HttpRequest request) where TEntity : class;
    }
}