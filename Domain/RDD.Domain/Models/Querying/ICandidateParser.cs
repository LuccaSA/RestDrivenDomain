using System.Collections.Generic;

namespace Rdd.Domain.Models.Querying
{
    public interface ICandidateParser
    {
        ICandidate<TEntity, TKey> Parse<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>;

        IEnumerable<ICandidate<TEntity, TKey>> ParseMany<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>;
    }
}