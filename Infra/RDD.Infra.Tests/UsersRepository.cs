using Rdd.Application;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using System.Linq;

namespace Rdd.Infra.Tests
{
    public class UsersRepository : OpenRepository<User>
    {
        public UsersRepository(IStorageService storageService, IRightExpressionsHelper<User> rightsService)
        : base(storageService, rightsService) { }

        protected override IQueryable<User> ApplyOrderBys(IQueryable<User> entities, Query<User> query)
        {
            if (!query.OrderBys.Any())
            {
                return entities.OrderBy(u => u.Id.ToString());
            }

            return base.ApplyOrderBys(entities, query);
        }
    }
}
