using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class WebPaginggTests : IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;
        private InMemoryStorageService _storage;
        private OpenRepository<User, Guid> _repo;
        private UsersCollection _collection;
        private HttpQuery<User, Guid> _httpQuery;

        public WebPaginggTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _storage = new InMemoryStorageService();
            _httpQuery = new HttpQuery<User, Guid>();
            _repo = new OpenRepository<User, Guid>(_storage, _fixture.RightsService, _httpQuery);
            _collection = new UsersCollection(_repo, _fixture.PatcherProvider, _fixture.Instanciator);
        }

        [Fact]
        public async void Default_Paging_should_be_0_to_10()
        {
            IEnumerable<User> users = User.GetManyRandomUsers(20);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            _httpQuery.Page = new Page(0, 10, int.MaxValue);
            ISelection<User> result = await _repo.GetAsync();

            Assert.Equal(0, _httpQuery.Page.Offset);
            Assert.Equal(10, _httpQuery.Page.Limit);
            Assert.Equal(10, result.Items.Count());
            Assert.Equal(20, result.Count);
        }

        [Fact]
        public async Task Paging_should_limit_to_1000_result()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                IEnumerable<User> users = User.GetManyRandomUsers(2000);
                _repo.AddRange(users);
                await _storage.SaveChangesAsync();

                var query = new HttpQuery<User, Guid> { Page = new Page(0, 1001, 1000) };
                await _repo.GetAsync();
            });
        }

        [Fact]
        public void Paging_should_start_at_0_result()
        {
            Assert.Throws<BadRequestException>(() => new HttpQuery<User, Guid> { Page = new Page(-10, 10, int.MaxValue) });
        }
    }
}