using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Infra.Tests.Storage
{
    public class StorageTests
    {
        private readonly EFStorageService _efStorage = new EFStorageService(new DataContext());
        private readonly InMemoryStorageService _inMemory = new InMemoryStorageService();

        [Fact]
        public void CstrThrowsWhenNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EFStorageService(null));
        }

        [Fact]
        public async Task EnumerateThrowsWhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _efStorage.EnumerateEntitiesAsync<User>(null));
        }

        [Fact]
        public async Task CountThrowsWhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _efStorage.CountAsync<User>(null));
        }

        [Fact]
        public async Task AnyThrowsWhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _efStorage.AnyAsync<User>(null));
        }

        [Fact]
        public async Task EnumerateDoesNotFailWhenQueryableIsNotFromEF()
        {
            var queryable = new List<User>().AsQueryable();
            var result = await _efStorage.EnumerateEntitiesAsync(queryable);
        }

        [Fact]
        public void InMemoryDeleteRange()
        {
            var user = new User();
            _inMemory.Add(user);
            Assert.Single(_inMemory.Set<User>());
            _inMemory.Remove(user);
            Assert.Empty(_inMemory.Set<User>());

            var users = new List<User> { new User(), new User() };
            _inMemory.AddRange(users);
            Assert.Equal(2, _inMemory.Set<User>().ToList().Count);
            _inMemory.RemoveRange(users);
            Assert.Empty(_inMemory.Set<User>());

        }

        [Fact]
        public async Task RepoOnNullFilterDoesNotFail()
        {
            var repo = new Repository<User>(new InMemoryStorageService(), new OpenRightExpressionsHelper<User>());
            await repo.AddAsync(new User(), new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });

            var count = await repo.CountAsync(new Query<User> { Filter = null });
            Assert.Equal(1, count);
        }
    }
}