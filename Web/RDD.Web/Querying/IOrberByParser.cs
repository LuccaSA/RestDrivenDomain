using System.Collections.Generic;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public interface IOrberByParser
    {
        IEnumerable<OrderBy<TEntity>> ParseOrderBys<TEntity>() where TEntity : class;
    }
}