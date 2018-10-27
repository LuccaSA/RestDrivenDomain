using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System;
using System.Linq;

namespace Rdd.Infra.Tests
{
    public class UsersRepository : OpenRepository<User, Guid>
    {
        public UsersRepository(IStorageService storageService, IRightExpressionsHelper<User> rightsService)
        : base(storageService, rightsService) { }

        protected override IQueryable<User> ApplyOrderBys(IQueryable<User> entities, IQuery<User> query)
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
