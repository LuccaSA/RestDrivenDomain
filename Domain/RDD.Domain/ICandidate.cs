using RDD.Domain.Json;
using System;
using System.Linq.Expressions;

namespace RDD.Domain
{
    public interface ICandidate<TEntity, TKey>
        where TEntity : IEntityBase<TKey>
    {
        TEntity Value { get; }
        JsonObject JsonValue { get; }
        bool HasProperty(Expression<Func<TEntity, object>> expression);
        bool HasId();
        TKey Id { get; }
    }
}
