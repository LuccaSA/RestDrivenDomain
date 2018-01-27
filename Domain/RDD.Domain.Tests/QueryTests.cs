using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.Storage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
    public class QueryTests : SingleContextTests
    {
        private readonly IRepository<User> _repo;
        private IReadOnlyRestCollection<User, int> _collection;
        private readonly IStorageService _storage;

        public QueryTests()
        {
            _storage = _newStorage(Guid.NewGuid().ToString());
            _repo = new Repository<User>(_storage, _execution, _combinationsHolder);
            _collection = new UsersCollection(_repo, _execution, _combinationsHolder);
        }

        [Fact]
        public void Cloning_query_should_not_clone_verb()
        {
            var query = new Query<User> { Verb = HttpVerbs.Put };
            var result = new Query<User>(query);

            Assert.Equal(HttpVerbs.Get, result.Verb);
        }

        [Fact]
        public void Cloning_query_should_not_clone_stopwatch()
        {
            var query = new Query<User> { Verb = HttpVerbs.Put };
            var result = new Query<User>(query);

            Assert.NotEqual(query.Watch, result.Watch);
        }

        [Fact]
        public async Task Stopwatch_should_work()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _execution, null);
                var users = new UsersCollection(repo, _execution, null);
                var appController = new UsersAppController(storage, users);

                //POST
                var postQuery = new Query<User>();
                postQuery.Options.CheckRights = false;

                await appController.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), postQuery);

                Assert.True(postQuery.Watch.ElapsedMilliseconds > 0);

                //GET
                var getQuery = new Query<User>();
                getQuery.Options.CheckRights = false;

                await appController.GetAsync(getQuery);

                Assert.True(getQuery.Watch.ElapsedMilliseconds > 0);

                //PUT
                var putQuery = new Query<User>();
                putQuery.Options.CheckRights = false;

                await appController.UpdateByIdAsync(3, PostedData.ParseJson(@"{ ""name"": ""newName"" }"), putQuery);

                Assert.True(putQuery.Watch.ElapsedMilliseconds > 0);

                //DELETE
                var deleteQuery = new Query<User>();
                deleteQuery.Options.CheckRights = false;

                await appController.DeleteByIdAsync(3, deleteQuery);

                Assert.True(deleteQuery.Watch.ElapsedMilliseconds > 0);
            }
        }
    }
}
