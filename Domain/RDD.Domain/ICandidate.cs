using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RDD.Domain
{
    public interface ICandidate<TEntity>
        where TEntity : class
    {
        TEntity Value { get; }
        bool HasProperty(Expression<Func<TEntity, object>> expression);
    }
}
