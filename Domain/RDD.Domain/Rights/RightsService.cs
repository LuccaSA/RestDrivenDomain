using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Rights
{
    public class RightsService : IRightsService
    {
        protected IPrincipal _principal;
        protected ICombinationsHolder _combinationsHolder;

        public RightsService(IPrincipal principal, ICombinationsHolder combinationsHolder)
        {
            _principal = principal ?? throw new ArgumentNullException(nameof(principal));
            _combinationsHolder = combinationsHolder ?? throw new ArgumentNullException(nameof(combinationsHolder));
        }

        public bool IsAllowed<T>(HttpVerbs verb) => _principal.HasAnyOperations(GetOperationIds<T>(verb));

        public virtual Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class
        {
            var operationIds = GetOperationIds<T>(query.Verb);
            if (!operationIds.Any())
            {
                throw new UnreachableEntityException(typeof(T));
            }

            return _principal.ApplyRights<T>(operationIds);
        }

        public virtual HashSet<int> GetOperationIds<T>(HttpVerbs verb)
        {
            var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(T) && c.Verb.HasVerb(verb));

            return new HashSet<int>(combinations.Select(c => c.Operation.Id));
        }
    }
}
