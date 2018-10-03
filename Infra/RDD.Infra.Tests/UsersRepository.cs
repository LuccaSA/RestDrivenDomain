using RDD.Application;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using System.Linq;

namespace RDD.Infra.Tests
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
