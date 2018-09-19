using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using System.Linq;
using RDD.Domain.Models.Querying;
using Xunit;

namespace RDD.Domain.Tests
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
        public async void NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new RightsServiceMock<ConcreteClassThree>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<ConcreteClassThree>(storage, rightsService);

            repo.Add(new ConcreteClassThree());
            repo.Add(new ConcreteClassThree());

            var collection = new ConcreteClassThreeCollection(repo);

            var result = await collection.GetAsync(new Query<ConcreteClassThree>());

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new RightsServiceMock<AbstractClass>();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<AbstractClass>(storage, rightsService);

            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassTwo());

            var collection = new AbstractClassCollection(repo);

            var result = await collection.GetAsync(new Query<AbstractClass>());

            Assert.Equal(3, result.Count());
        }
    }
}
