using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Linq;

namespace Rdd.Infra.Tests
{
    public class UsersRepository : OpenRepository<User>
    {
        private static readonly IExpressionTree WhiteList = new ExpressionParser().ParseTree<User>(nameof(User.Department));
        protected override IExpressionTree IncludeWhiteList => WhiteList;

        public UsersRepository(IStorageService storageService, IRightExpressionsHelper<User> rightsService)
        : base(storageService, rightsService) { }

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