using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Domain.Tests
{
    internal class AbstractClassCollection : ReadOnlyRestCollection<AbstractClass, int>
    {
        public AbstractClassCollection(IRepository<AbstractClass> repository)
            : base(repository) { }
    }

    internal class ConcreteClassThreeCollection : ReadOnlyRestCollection<ConcreteClassThree, int>
    {
        public ConcreteClassThreeCollection(IRepository<ConcreteClassThree> repository)
            : base(repository) { }
    }

    public class AbstractEntityTests
    {
        [Fact]
        public async Task NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new OpenRightExpressionsHelper<ConcreteClassThree>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<ConcreteClassThree>(storage, rightsService);

            await repo.AddAsync(new ConcreteClassThree(), new Query<ConcreteClassThree> { Verb = Helpers.HttpVerbs.Post });
            await repo.AddAsync(new ConcreteClassThree(), new Query<ConcreteClassThree> { Verb = Helpers.HttpVerbs.Post });

            var collection = new ConcreteClassThreeCollection(repo);

            var result = await collection.GetAsync(new Query<ConcreteClassThree>());

            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async Task AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new OpenRightExpressionsHelper<AbstractClass>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<AbstractClass>(storage, rightsService);

            await repo.AddAsync(new ConcreteClassOne(), new Query<AbstractClass> { Verb = Helpers.HttpVerbs.Post });
            await repo.AddAsync(new ConcreteClassOne(), new Query<AbstractClass> { Verb = Helpers.HttpVerbs.Post });
            await repo.AddAsync(new ConcreteClassTwo(), new Query<AbstractClass> { Verb = Helpers.HttpVerbs.Post });

            var collection = new AbstractClassCollection(repo);

            var result = await collection.GetAsync(new Query<AbstractClass>());

            Assert.Equal(3, result.Items.Count());
        }
    }
}
