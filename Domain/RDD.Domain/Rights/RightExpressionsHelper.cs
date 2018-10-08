using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Domain.Rights
{
    public class RightExpressionsHelper<T> : IRightExpressionsHelper<T>
         where T : class
    {
        protected IPrincipal Principal { get; set; }
        protected ICombinationsHolder CombinationsHolder { get; set; }

        public RightExpressionsHelper(IPrincipal principal, ICombinationsHolder combinationsHolder)
        {
            Principal = principal;
            CombinationsHolder = combinationsHolder ?? throw new ArgumentNullException(nameof(combinationsHolder));
        }

        public virtual Expression<Func<T, bool>> GetFilter(Query<T> query)
        {
            if (Principal == null)
            {
                throw new ForbiddenException("Anonymous query is forbidden");
            }

            var operationIds = GetOperationIds(query.Verb);
            if (!operationIds.Any())
            {
                throw new UnreachableEntityException(typeof(T));
            }

            return t => true;
        }

        public virtual HashSet<int> GetOperationIds(HttpVerbs verb)
        {
            var combinations = CombinationsHolder.Combinations.Where(c => c.Subject == typeof(T) && c.Verb.HasFlag(verb));
            return new HashSet<int>(combinations.Select(c => c.Operation.Id));
        }
    }
}