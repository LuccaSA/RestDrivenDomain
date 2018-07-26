using Microsoft.EntityFrameworkCore;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra;
using RDD.Infra.Storage;
using RDD.Web.Querying;
using System;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    public class CollectionPropertiesTests : SingleContextTests
    {
        private readonly IRepository<User> _repo;
        private readonly IReadOnlyRestCollection<User,int> _collection;
        private readonly IStorageService _storage;

        public CollectionPropertiesTests()
        {
            _storage = _newStorage(Guid.NewGuid().ToString());
            _repo = new OpenRepository<User>(_storage, _rightsService, QueryContex.Request);
            _collection = new UsersCollection(_repo, _patcherProvider, Instanciator, QueryContex);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10_when_10_entities()
        {
            var users = User.GetManyRandomUsers(10);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var result = await _collection.GetAsync(new Query<User>());

            Assert.Equal(10, result.Count);
        }

        [Fact]
        public async void Count_of_collection_should_tell_100_when_100_entities()
        {
            var users = User.GetManyRandomUsers(100);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var result = await _collection.GetAsync(new Query<User>());
            
            Assert.Equal(100, QueryContex.Response.TotalCount);
        }

        [Fact]
        public async void Count_of_collection_should_tell_10000_when_10000_entities()
        {
            var users = User.GetManyRandomUsers(10000);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var result = await _collection.GetAsync(new Query<User>());

            Assert.Equal(10, result.Count);
            Assert.Equal(10000, QueryContex.Response.TotalCount);
        }
    }
}
