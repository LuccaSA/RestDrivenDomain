using RDD.Domain.Tests;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using RDD.Web.Querying;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests
{
    public class CollectionPropertiesTests : IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;
        private InMemoryStorageService _storage;
        private OpenRepository<User> _repo;
        private UsersCollection _collection;

        public CollectionPropertiesTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _storage = new InMemoryStorageService();
            _repo = new OpenRepository<User>(_storage, _fixture.RightsService);
            _collection = new UsersCollection(_repo, _fixture.PatcherProvider, _fixture.Instanciator);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10_when_10_entities()
        {
            var users = User.GetManyRandomUsers(10);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new WebQuery<User>());
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_100_when_100_entities()
        {
            var users = User.GetManyRandomUsers(100);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new WebQuery<User>());
            Assert.Equal(100, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10000_when_10000_entities()
        {
            var users = User.GetManyRandomUsers(10000);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new WebQuery<User>());
            Assert.Equal(10, result.Items.Count());
            Assert.Equal(10000, result.Count);
        }
    }
}