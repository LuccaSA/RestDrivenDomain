using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra;
using Xunit;

namespace RDD.Domain.Tests
{
    public class PagingTests : SingleContextTests
    {
        public PagingTests()
        {
            _queryContext = new QueryContext(new QueryRequest(), new QueryResponse());
            _storage = _newStorage(Guid.NewGuid().ToString());
            _repo = new OpenRepository<User>(_storage, _rightsService, _queryContext.Request);
            _collection = new UsersCollection(_repo, _patcherProvider, Instanciator, _queryContext);
        }

        private readonly QueryContext _queryContext;
        private readonly IRepository<User> _repo;
        private readonly IReadOnlyRestCollection<User, int> _collection;
        private readonly IStorageService _storage;

        [Fact]
        public async void Default_Paging_should_be_0_to_10()
        {
            IEnumerable<User> users = User.GetManyRandomUsers(20);
            _repo.AddRange(users);
            await _storage.SaveChangesAsync();

            var query = new Query<User>();
            var result = await _collection.GetAsync(query);

            Assert.Equal(0, _queryContext.Request.PageOffset);
            Assert.Equal(10, _queryContext.Request.ItemPerPage);

            Assert.Equal(10, result.Count);
            Assert.Equal(20, _queryContext.Response.TotalCount);
        }

        [Fact]
        public async Task Paging_should_limit_to_1000_result()
        {
            await Assert.ThrowsAsync<OutOfRangeException>(async () =>
            {
                IEnumerable<User> users = User.GetManyRandomUsers(2000);
                _repo.AddRange(users);
                await _storage.SaveChangesAsync();

                _queryContext.Request.ItemPerPage = 1001;

                var query = new Query<User>();
                await _collection.GetAsync(query);
            });
        }
    }
}
