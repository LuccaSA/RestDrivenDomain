using System;
using System.Linq.Expressions;

namespace Rdd.Domain.Models
{
    public class DefaultInstanciator<TEntity> : IInstanciator<TEntity>
        where TEntity : class, new()
    {
        private readonly Func<TEntity> _new;

        public DefaultInstanciator()
        {
            Expression<Func<TEntity>> NewExpression = () => new TEntity();
            _new = NewExpression.Compile();
        }

        public TEntity InstanciateNew(ICandidate<TEntity> candidate) => _new();// new TEntity() is 10X slower
    }
}