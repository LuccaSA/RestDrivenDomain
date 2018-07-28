using System.Collections.Generic;
using RDD.Infra.Web.Models;

namespace RDD.Web.Querying
{
    public interface IWebFilterParser
    {
        IEnumerable<WebFilter<TEntity>> ParseWebFilters<TEntity>() where TEntity : class;
    }
}