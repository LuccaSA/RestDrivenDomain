using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class WebPaginggTests : IClassFixture<DefaultFixture>
    {
        private readonly DefaultFixture _fixture;
        private readonly InMemoryStorageService _storage;
        private readonly OpenRepository<User> _repo;
        private readonly UsersCollection _collection;

        public WebPaginggTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _storage = new InMemoryStorageService();
            _repo = new OpenRepository<User>(_storage, _fixture.RightsService);
            _collection = new UsersCollection(_repo, _fixture.PatcherProvider, _fixture.Instanciator);
        }

        [Fact]
        public async Task Default_Paging_should_be_0_to_10()
        {
            IEnumerable<User> users = User.GetManyRandomUsers(20);
            await _repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
            await _storage.SaveChangesAsync();

            var query = new Query<User>
            {
                Page = new Page(0, 10, int.MaxValue)
            };
            ISelection<User> result = await _collection.GetAsync(query);

            Assert.Equal(0, query.Page.Offset);
            Assert.Equal(10, query.Page.Limit);
            Assert.Equal(10, result.Items.Count());
            Assert.Equal(20, result.Count);
        }

        [Fact]
        public async Task Paging_should_limit_to_1000_result()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                IEnumerable<User> users = User.GetManyRandomUsers(2000);
                await _repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await _storage.SaveChangesAsync();

                var query = new Query<User> { Page = new Page(0, 1001, 1000) };
                await _collection.GetAsync(query);
            });
        }

        [Fact]
        public void Paging_should_start_at_0_result()
        {
            Assert.Throws<BadRequestException>(() => new Query<User> { Page = new Page(-10, 10, int.MaxValue) });
        }
    }
}