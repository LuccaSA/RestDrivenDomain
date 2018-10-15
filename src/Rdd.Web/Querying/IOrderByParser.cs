using System.Collections.Generic;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IOrderByParser
    {
        List<OrderBy<TEntity>> Parse<TEntity>(string value) where TEntity : class;
    }
}