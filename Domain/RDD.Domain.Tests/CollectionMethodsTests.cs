using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
    public class CollectionMethodsTests : SingleContextTests
    {
        [Fact]
        public async Task GetById_SHOULD_throw_exception_WHEN_id_does_not_exist()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 1 };
                var repo = new OpenRepository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollection(repo, _execution, _combinationsHolder);

                await users.CreateAsync(user);

                await storage.SaveChangesAsync();

                await Assert.ThrowsAsync<NotFoundException>(() => users.GetByIdAsync(0));
            }
        }

        [Fact]
        public async Task TryGetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 2 };
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollection(repo, _execution, _combinationsHolder);

                await users.CreateAsync(user);

                await storage.SaveChangesAsync();

                Assert.Null(await users.TryGetByIdAsync(0));
            }
        }

        [Fact]
        public async Task Put_SHOULD_throw_notfound_exception_WHEN_unexisting_entity_()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                _execution.curPrincipal = new WebService { Id = 1, AppOperations = new HashSet<int>() { 1 } };

                var mock = new Mock<ICombinationsHolder>();
                mock.Setup(h => h.Combinations)
                    .Returns(new HashSet<Combination>() {
                        new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerbs.Post },
                        new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerbs.Put }
                    });
                var combinationsHolder = mock.Object;

                var user = new User { Id = 3 };
                var repo = new Repository<User>(storage, _execution, combinationsHolder);
                var users = new UsersCollection(repo, _execution, combinationsHolder);
                var app = new UsersAppController(storage, users);

                await app.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), new Query<User>());

                await Assert.ThrowsAsync<NotFoundException>(() => app.UpdateByIdAsync(0, PostedData.ParseJson(@"{ ""name"": ""new name"" }"), new Query<User>()));
            }
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsNotOverridenAndEntityHasAParameterlessConstructor()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _execution, null);
                var users = new UsersCollection(repo, _execution, null);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                await users.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), query);
            }
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsOverriden()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _execution, null);
                var users = new UsersCollectionWithOverride(repo, _execution, null);
                var query = new Query<User>();
                query.Options.CheckRights = false;
                
                await users.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), query);
            }
        }


        [Fact]
        public async Task Post_SHOULD_fail_WHEN_InstantiateEntityIsNotOverridenAndEntityHasParametersInConstructor()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<UserWithParameters>(storage, _execution, null);
                var users = new UsersCollectionWithParameters(repo, _execution, null);
                var query = new Query<UserWithParameters>();
                query.Options.CheckRights = false;

                await Assert.ThrowsAsync<MissingMethodException>(
                    () => users.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), query)
                );
            }
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsOverridenAndEntityHasParametersInConstructor()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<UserWithParameters>(storage, _execution, null);
                var users = new UsersCollectionWithParametersAndOverride(repo, _execution, null);
                var query = new Query<UserWithParameters>();
                query.Options.CheckRights = false;

                var result = await users.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3, ""name"": ""John"" }"), query);

                Assert.Equal(3, result.Id);
                Assert.Equal("John", result.Name);
            }
        }

        [Fact]
        public async Task AnyAsync_should_work()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 2 };
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollection(repo, _execution, _combinationsHolder);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                await users.CreateAsync(user, query);

                await storage.SaveChangesAsync();

                var any = await users.AnyAsync(query);

                Assert.True(any);
            }
        }

        [Fact]
        public async Task Put_serializedEntity_should_work()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 2, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollection(repo, _execution, _combinationsHolder);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                await users.CreateAsync(user, query);

                await storage.SaveChangesAsync();

                await users.UpdateByIdAsync(2, user, query);

                Assert.True(true);
            }
        }

        [Fact]
        public async Task Get_withFilters_shouldWork()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 2, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollection(repo, _execution, _combinationsHolder);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                await users.CreateAsync(user, query);

                await storage.SaveChangesAsync();

                query.Filters.Add(new Filter<User>(new PropertySelector<User>(u => u.TwitterUri.Host), FilterOperand.Equals, new List<string>() { "twitter.com" }));

                var results = await users.GetAsync(query);

                Assert.Equal(1, results.Count);
            }
        }
    }
}
