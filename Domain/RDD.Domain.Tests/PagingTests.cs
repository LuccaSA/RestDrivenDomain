using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PagingTests : SingleContextTests
    {
        public PagingTests()
        {
            _storage = _newStorage(Guid.NewGuid().ToString());
            _repo = new OpenRepository<User>(_storage, _execution, _combinationsHolder);
            _collection = new UsersCollection(_repo, _execution, _combinationsHolder);
        }

        private readonly IRepository<User> _repo;
        private readonly IReadOnlyRestCollection<User, int> _collection;
        private readonly IStorageService _storage;

        [Fact]
        public void Changing_query_page_count_should_not_affect_another_query()
        {
            var query1 = new Query<User>();
            query1.Page.TotalCount = 20;

            var query2 = new Query<User>();
            query2.Page.TotalCount = 10;

            Assert.Equal(20, query1.Page.TotalCount);
        }

        [Fact]
        public async void Default_Paging_should_be_0_to_10()
        {
            IEnumerable<User> users = User.GetManyRandomUsers(20);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var query = new Query<User>();
            ISelection<User> result = await _collection.GetAsync(query);

            Assert.Equal(0, query.Page.Offset);
            Assert.Equal(10, query.Page.Limit);
            Assert.Equal(10, result.Items.Count());
            Assert.Equal(20, result.Count);
        }

        [Fact]
        public async Task Paging_should_limit_to_1000_result()
        {
            await Assert.ThrowsAsync<OutOfRangeException>(async () =>
            {
                IEnumerable<User> users = User.GetManyRandomUsers(2000);
                _repo.AddRange(users);
                await _storage.SaveChangesAsync();

                var query = new Query<User> { Page = new Page(0, 1001) };
                ISelection<User> result = await _collection.GetAsync(query);
            });
        }
    }
}
