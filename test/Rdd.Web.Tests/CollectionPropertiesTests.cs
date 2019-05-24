using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Linq;
using Rdd.Domain.Models.Querying;
using Xunit;

namespace Rdd.Web.Tests
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
            await _repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new Query<User>());
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_100_when_100_entities()
        {
            var users = User.GetManyRandomUsers(100);
            await _repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new Query<User>());
            Assert.Equal(100, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10000_when_10000_entities()
        {
            var users = User.GetManyRandomUsers(10000);
            await _repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
            await _storage.SaveChangesAsync();
            var result = await _collection.GetAsync(new Query<User>()
            {
                Page = new Page(0, 10,int.MaxValue)
            });
            Assert.Equal(10, result.Items.Count());
            Assert.Equal(10000, result.Count);
        }
    }
}