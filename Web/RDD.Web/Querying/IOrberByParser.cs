using System.Collections.Generic;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public interface IOrderByParser
    {
        IEnumerable<OrderBy<TEntity>> ParseOrderBys<TEntity>() where TEntity : class;
    }
}