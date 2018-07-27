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
            _storage = _newStorage(Guid.NewGuid().ToString());
            _repo = new OpenRepository<User>(_storage, _rightsService);
            _collection = new UsersCollection(_repo, _patcherProvider, Instanciator);
        }
         
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
            
            Assert.Equal(0, query.Paging.PageOffset);
            Assert.Equal(10, query.Paging.ItemPerPage);
            
            Assert.Equal(10, result.Count);
            Assert.Equal(20, query.QueryMetadata.TotalCount);
        }

        [Fact]
        public async Task Paging_should_limit_to_1000_result()
        {
            await Assert.ThrowsAsync<OutOfRangeException>(async () =>
            {
                IEnumerable<User> users = User.GetManyRandomUsers(2000);
                _repo.AddRange(users);
                await _storage.SaveChangesAsync();

                var query = new Query<User>();
                query.Paging.ItemPerPage = 1001;
                await _collection.GetAsync(query);
            });
        }
    }
}
