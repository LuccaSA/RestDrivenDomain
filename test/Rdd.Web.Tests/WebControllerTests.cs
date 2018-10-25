using Rdd.Application.Controllers;
using Rdd.Domain.Models;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
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
            using (var storage = new InMemoryStorageService())
            {
                var query = new HttpQuery<IUser, int>();
                query.Options.CheckRights = false;
                var repository = new Repository<IUser, int>(storage, null, query);

                repository.Add(new User { Id = 1 });
                repository.Add(new AnotherUser { Id = 2 });

                var controller = new UserWebController(repository, QueryParserHelper.GetQueryParser<IUser, int>(), query);

                var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

                Assert.Equal(2, results.Count());
            }
        }
    }
}
