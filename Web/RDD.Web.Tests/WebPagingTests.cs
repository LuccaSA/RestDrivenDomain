using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using RDD.Web.Querying;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Web.Tests
{
    public class WebPaginggTests : IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;
        private InMemoryStorageService _storage;
        private OpenRepository<User> _repo;
        private UsersCollection _collection;

        public WebPaginggTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _storage = new InMemoryStorageService();
            _repo = new OpenRepository<User>(_storage, _fixture.RightsService);
            _collection = new UsersCollection(_repo, _fixture.PatcherProvider, _fixture.Instanciator);
        }

        [Fact]
        public void Changing_query_page_count_should_not_affect_another_query()
        {
            var query1 = new WebQuery<User>();
            query1.Page.TotalCount = 20;

            var query2 = new WebQuery<User>();
            query2.Page.TotalCount = 10;

            Assert.Equal(20, query1.Page.TotalCount);
        }

        [Fact]
        public async void Default_Paging_should_be_0_to_10()
        {
            IEnumerable<User> users = User.GetManyRandomUsers(20);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var query = new WebQuery<User>();
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
                _repo.AddRange(users);
                await _storage.SaveChangesAsync();

                var query = new WebQuery<User> { Page = new WebPage(0, 1001) };
                ISelection<User> result = await _collection.GetAsync(query);
            });
        }

        [Fact]
        public void Paging_should_start_at_0_result()
        {
            Assert.Throws<BadRequestException>(() => new Query<User> { Page = new WebPage(-10, 10) });
        }
    }
}