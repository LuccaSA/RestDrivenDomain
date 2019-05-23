using Rdd.Application.Controllers;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using Rdd.Web.Tests.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class WebControllerTests
    {
        [Fact]
        public async Task WebControllerShouldWorkOnInterfaces()
        {
            var storage = new InMemoryStorageService();

            var repository = new Repository<IUser>(storage, new OpenRightExpressionsHelper<IUser>());
            var collection = new ReadOnlyRestCollection<IUser, int>(repository);
            var appController = new ReadOnlyAppController<IUser, int>(collection);

            await repository.AddAsync(new User { Id = 1 }, new Query<IUser> { Verb = Domain.Helpers.HttpVerbs.Post });
            await repository.AddAsync(new AnotherUser { Id = 2 }, new Query<IUser> { Verb = Domain.Helpers.HttpVerbs.Post });

            var controller = new UserWebController(appController, QueryParserHelper.GetQueryParser<IUser>());

            var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

            Assert.Equal(2, results.Count());
        }
    }
}
