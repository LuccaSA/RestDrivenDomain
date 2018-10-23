using Microsoft.AspNetCore.Http;
using Rdd.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Web.Querying
{
    public interface ICandidateParser
    {
        ICandidate<TEntity, TKey> Parse<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>;

        IEnumerable<ICandidate<TEntity, TKey>> ParseMany<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>;

        Task<ICandidate<TEntity, TKey>> ParseAsync<TEntity, TKey>(HttpRequest request)
           where TEntity : class, IPrimaryKey<TKey>;

        Task<IEnumerable<ICandidate<TEntity, TKey>>> ParseManyAsync<TEntity, TKey>(HttpRequest request)
           where TEntity : class, IPrimaryKey<TKey>;
    }
}