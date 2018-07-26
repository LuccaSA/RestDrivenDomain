using RDD.Domain.Json;
using System;
using System.Linq.Expressions;

namespace RDD.Domain
{
    public interface ICandidate<TEntity>
    {
        object Id { get; }
        TEntity Value { get; }
        JsonObject JsonValue { get; }

        bool HasProperty(Expression<Func<TEntity, object>> expression);
        bool HasId();
    }

    public interface ICandidate<TEntity, TKey> : ICandidate<TEntity>
        where TEntity : IPrimaryKey<TKey>
    {
        new TKey Id { get; }
    }
}