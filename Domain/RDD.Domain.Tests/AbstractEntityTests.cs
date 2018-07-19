using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    internal class AbstractClassCollection : ReadOnlyRestCollection<AbstractClass, int>
    {
        public AbstractClassCollection(IRepository<AbstractClass> repository, IRightsService rightsService)
            : base(repository, rightsService) { }
    }

    internal class ConcreteClassThreeCollection : ReadOnlyRestCollection<ConcreteClassThree, int>
    {
        public ConcreteClassThreeCollection(IRepository<ConcreteClassThree> repository, IRightsService rightsService)
            : base(repository, rightsService) { }
    }

    public class AbstractEntityTests
    {
        [Fact]
        public async void NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new RightsServiceMock();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<ConcreteClassThree>(storage, rightsService);

            repo.Add(new ConcreteClassThree());
            repo.Add(new ConcreteClassThree());

            var collection = new ConcreteClassThreeCollection(repo, rightsService);

            var result = await collection.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
        {
            var rightsService = new RightsServiceMock();
            var storage = new InMemoryStorageService();
            var repo = new OpenRepository<AbstractClass>(storage, rightsService);

            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassOne());
            repo.Add(new ConcreteClassTwo());

            var collection = new AbstractClassCollection(repo, rightsService);

            var result = await collection.GetAllAsync();

            Assert.Equal(3, result.Count());
        }
    }
}
