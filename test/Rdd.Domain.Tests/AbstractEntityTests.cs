using Rdd.Domain.Models;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Rights;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System.Linq;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class AbstractEntityTests
    {
        [Fact]
        public async void NonAbstractRepository_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new OpenRightExpressionsHelper<ConcreteClassThree>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<ConcreteClassThree, int>(storage, rightsService, new HttpQuery<ConcreteClassThree, int>());

            repo.Add(new ConcreteClassThree());
            repo.Add(new ConcreteClassThree());

            var result = await repo.GetAsync();

            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async void AbstractRepository_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new OpenRightExpressionsHelper<AbstractClass>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<AbstractClass, int>(storage, rightsService, new HttpQuery<AbstractClass, int>());

            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassTwo());

            var result = await repo.GetAsync();

            Assert.Equal(3, result.Items.Count());
        }
    }
}
