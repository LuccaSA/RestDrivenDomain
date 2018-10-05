using Moq;
using RDD.Application.Controllers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Serialization;
using RDD.Web.Tests.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Web.Tests
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

                var controller = new UserWebController(appController, new QueryParser<IUser>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<IUser>()));

                var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

                Assert.Equal(2, results.Count());
            }
        }
    }
}
