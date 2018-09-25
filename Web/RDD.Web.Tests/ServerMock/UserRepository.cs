using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Tests.Models;

namespace RDD.Web.Tests.ServerMock
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(IStorageService storageService, IRightExpressionsHelper<User> rightExpressionsHelper)
            : base(storageService, rightExpressionsHelper) { }

        public override async Task<IReadOnlyCollection<User>> GetAsync(Query<User> query)
        {
            var result = await base.GetAsync(query);
            return result;
        }

        protected override IQueryable<User> ApplyIncludes(IQueryable<User> entities, Query<User> query)
        {
            return entities.Include(u => u.Department)
                .Include(u => u.MyValueObject)
                .Include(u => u.Files)
                .ThenInclude(f => f.FileDescriptor);
        }
    }
}