using System.Collections.Generic;
using RDD.Domain;

namespace RDD.Web.Helpers
{
    public interface ICandidateFactory<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
    {
        ICandidate<TEntity, TKey> CreateCandidate();
        IEnumerable<ICandidate<TEntity, TKey>> CreateCandidates();
    }
}