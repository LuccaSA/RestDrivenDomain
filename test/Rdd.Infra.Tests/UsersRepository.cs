using Rdd.Domain.Tests.Models;
using Rdd.Infra.Rights;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System;
using System.Linq;

namespace Rdd.Infra.Tests
{
    public class UsersRepository : OpenRepository<User, Guid>
    {
        public UsersRepository(IStorageService storageService, IRightExpressionsHelper<User> rightsService, HttpQuery<User, Guid> httpQuery)
        : base(storageService, rightsService, httpQuery) { }

        protected override IQueryable<User> ApplyOrderBys(IQueryable<User> entities, Query<User> query)
        {
            if (!query.OrderBys.Any())
            {
                var orderById = OrderBy<User>.Ascending(u => u.Id);

                return orderById.ApplyOrderBy(entities);
            }

            return base.ApplyOrderBys(entities, query);
        }
    }
}
