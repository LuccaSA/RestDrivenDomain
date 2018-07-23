using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Rights
{
    public class RightExpressionsHelper : IRightExpressionsHelper
    {
        protected IPrincipal _principal;
        protected ICombinationsHolder _combinationsHolder;

        public RightExpressionsHelper(IPrincipal principal, ICombinationsHolder combinationsHolder)
        {
            _principal = principal;
            _combinationsHolder = combinationsHolder ?? throw new ArgumentNullException(nameof(combinationsHolder));
        }

        public virtual Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class
        {
            if (_principal == null)
            {
                throw new ForbiddenException("Anonymous query is forbidden");
            }

            var operationIds = GetOperationIds<T>(query.Verb);
            if (!operationIds.Any())
            {
                throw new UnreachableEntityException(typeof(T));
            }

            return t => true;
        }

        public virtual HashSet<int> GetOperationIds<T>(HttpVerbs verb)
        {
            var combinations = _combinationsHolder.Combinations.Where(c => c.Subject == typeof(T) && c.Verb.HasVerb(verb));
            return new HashSet<int>(combinations.Select(c => c.Operation.Id));
        }
    }
}