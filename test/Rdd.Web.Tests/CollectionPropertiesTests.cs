using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Linq;
using Rdd.Domain.Models.Querying;
using Xunit;
using System;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Tests
{
    public class CollectionPropertiesTests : IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;
        private InMemoryStorageService _storage;
        private OpenRepository<User, Guid> _repo;
        private UsersCollection _collection;
        private HttpQuery<User, Guid> _httpQuery;

        public CollectionPropertiesTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _storage = new InMemoryStorageService();
            _httpQuery = new HttpQuery<User, Guid>();
            _repo = new OpenRepository<User, Guid>(_storage, _fixture.RightsService, _httpQuery);
            _collection = new UsersCollection(_repo, _fixture.PatcherProvider, _fixture.Instanciator);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10_when_10_entities()
        {
            var users = User.GetManyRandomUsers(10);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            var result = await _repo.GetAsync();
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_100_when_100_entities()
        {
            var users = User.GetManyRandomUsers(100);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            var result = await _repo.GetAsync();
            Assert.Equal(100, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10000_when_10000_entities()
        {
            var users = User.GetManyRandomUsers(10000);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();
            _httpQuery.Page = new Page(0, 10, int.MaxValue);
            var result = await _repo.GetAsync();

            Assert.Equal(10, result.Items.Count());
            Assert.Equal(10000, result.Count);
        }
    }
}