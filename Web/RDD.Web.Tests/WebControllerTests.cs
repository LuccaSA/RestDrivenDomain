using Rdd.Application.Controllers;
using Rdd.Domain.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Tests.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rdd.Web.Querying;
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
                var repository = new Repository<IUser>(storage, null);
                var collection = new ReadOnlyRestCollection<IUser, int>(repository);
                var appController = new ReadOnlyAppController<IUser, int>(collection);

                repository.Add(new User { Id = 1 });
                repository.Add(new AnotherUser { Id = 2 });

                var httpContextHelper = new HttpContextHelper(new HttpContextAccessor
                {
                    HttpContext = new DefaultHttpContext()
                });

                var query = new WebQueryFactory<IUser, int>(Options.Create(new RddOptions()));
 
                var apiHelper = new ApiHelper<IUser, int>(httpContextHelper, query);

                var controller = new UserWebController(appController, apiHelper);

                var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

                Assert.Equal(2, results.Count());
            }
        }
    }
}
