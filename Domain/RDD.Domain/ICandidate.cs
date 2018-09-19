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

        bool HasProperty<TProp>(Expression<Func<TEntity, TProp>> expression);
        bool HasId();
    }

    public interface ICandidate<TEntity, TKey> : ICandidate<TEntity>
        where TEntity : IPrimaryKey<TKey>
    {
        new TKey Id { get; }
    }
}