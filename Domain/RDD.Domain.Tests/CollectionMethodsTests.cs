using Moq;
using Newtonsoft.Json;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using RDD.Web.Models;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace RDD.Domain.Tests
{
    public class CollectionMethodsTests : SingleContextTests
    {
        [Fact]
        public async Task GetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);

                Assert.Null(await users.GetByIdAsync(0, new Query<User>()));
            }
        }

        [Fact]
        public async Task Put_SHOULD_NOT_throw_notfound_exception_WHEN_unexisting_entity_()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var mock = new Mock<ICombinationsHolder>();
                mock.Setup(h => h.Combinations)
                    .Returns(new HashSet<Combination>() {
                        new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerbs.Post },
                        new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerbs.Put }
                    });
                var rightService = new RightExpressionsHelper<User>(new WebService { Id = 1, AppOperations = new HashSet<int> { 1 } }, mock.Object);

                var user = new User { Id = 3 };
                var repo = new Repository<User>(storage, rightService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);
                var app = new UsersAppController(storage, users);

                await app.CreateAsync(Candidate<User, int>.Parse(@"{ ""id"": 3 }"), new Query<User>());

                await app.UpdateByIdAsync(0, Candidate<User, int>.Parse(@"{ ""name"": ""new name"" }"), new Query<User>());
            }
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsNotOverridenAndEntityHasAParameterlessConstructor()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);
                var query = new Query<User>(){CheckRights = false}; 

                await users.CreateAsync(Candidate<User, int>.Parse(@"{ ""id"": 3 }"), query);
            }
        }

        class InstanciatorImplementation : IInstanciator<UserWithParameters>
        {
            public UserWithParameters InstanciateNew(ICandidate<UserWithParameters> candidate)
            {
                var id = candidate.Value.Id;
                var name = candidate.Value.Name;

                return new UserWithParameters(id, name);
            }
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsOverridenAndEntityHasParametersInConstructor()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<UserWithParameters>(storage, new RightsServiceMock<UserWithParameters>());
                var users = new UsersCollectionWithParameters(repo, _patcherProvider, new InstanciatorImplementation());
                var query = new Query<UserWithParameters>(){ CheckRights =false}; 

                var result = await users.CreateAsync(Candidate<UserWithParameters, int>.Parse(@"{ ""id"": 3, ""name"": ""John"" }"), query);

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
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);
                var query = new Query<User>(){ CheckRights = false }; 

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
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);
                var query = new Query<User>() { CheckRights = false }; 

                await users.CreateAsync(user, query);

                await storage.SaveChangesAsync();

                await users.UpdateByIdAsync(2, Candidate<User, int>.Parse(JsonConvert.SerializeObject(user)), query);

                Assert.True(true);
            }
        }

        [Fact]
        public async Task Get_withFilters_shouldWork()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var user = new User { Id = 2, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollection(repo, _patcherProvider, Instanciator);
                var query = new Query<User>() { CheckRights = false }; 

                storage.Add(user);
                await storage.SaveChangesAsync();

                query = new Query<User>(query, u => u.TwitterUri == new Uri("https://twitter.com"));

                var results = await users.GetAsync(query);

                Assert.Equal(1, results.Count());
            }
        }
    }
}