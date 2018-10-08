using Rdd.Application.Controllers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Storage;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class WebControllerTests
    {
        QueryParser<IUser> GetQueryParser()
               => new QueryParser<IUser>(new WebFilterConverter<IUser>(), new PagingParser(), new FilterParser(new StringConverter(), new ExpressionParser()), new FieldsParser(new ExpressionParser()), new OrderByParser(new ExpressionParser()));

        [Fact]
        public async Task WebControllerShouldWorkOnInterfaces()
        {
            using (var storage = new InMemoryStorageService())
            {
                var repository = new Repository<IUser>(storage, null);
                var collection = new ReadOnlyRestCollection<IUser, int>(repository);
                var appController = new ReadOnlyAppController<IUser, int>(collection);

                repository.Add(new User { Id = 1 });
                repository.Add(new AnotherUser { Id = 2 });

                var controller = new UserWebController(appController, GetQueryParser());

                var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

                Assert.Equal(2, results.Count());
            }
        }
    }
}
