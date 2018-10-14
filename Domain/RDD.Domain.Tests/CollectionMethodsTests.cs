using Moq;
using Newtonsoft.Json;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using Rdd.Domain.Mocks;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class CollectionMethodsTests 
    {
        private readonly DefaultFixture _fixture;

        public CollectionMethodsTests()
        {
            _fixture = new DefaultFixture();
        }

        [Fact]
        public async Task GetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
        {
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);

            Assert.Null(await users.GetByIdAsync(Guid.NewGuid(), new Query<User>()));
        }

        [Fact]
        public async Task Put_SHOULD_NOT_throw_notfound_exception_WHEN_unexisting_entity_()
        {
            Expression<Func<User, bool>> trueFilter = t => true;
            var rightService = new Mock<IRightExpressionsHelper<User>>();
            rightService.Setup(s => s.GetFilter(It.IsAny<Query<User>>())).Returns(trueFilter);

            var id = Guid.NewGuid();
            var user = new User { Id = id };
            var repo = new Repository<User>(_fixture.InMemoryStorage, rightService.Object);
            var users = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var app = new UsersAppController(_fixture.InMemoryStorage, users);

            await app.CreateAsync(Candidate<User, Guid>.Parse($@"{{ ""id"": ""{id}"" }}"), new Query<User>());

            await app.UpdateByIdAsync(Guid.NewGuid(), Candidate<User, Guid>.Parse(@"{ ""name"": ""new name"" }"), new Query<User>());
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsNotOverridenAndEntityHasAParameterlessConstructor()
        {
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.CheckRights = false;

            await users.CreateAsync(Candidate<User, Guid>.Parse($@"{{ ""id"": ""{Guid.NewGuid()}"" }}"), query);
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
            var repo = new Repository<UserWithParameters>(_fixture.InMemoryStorage, new RightsServiceMock<UserWithParameters>());
            var users = new UsersCollectionWithParameters(repo, _fixture.PatcherProvider, new InstanciatorImplementation());
            var query = new Query<UserWithParameters>();
            query.Options.CheckRights = false;

            var result = await users.CreateAsync(Candidate<UserWithParameters, int>.Parse($@"{{ ""id"": 3, ""name"": ""John"" }}"), query);

            Assert.Equal(3, result.Id);
            Assert.Equal("John", result.Name);
        }

        [Fact]
        public async Task AnyAsync_should_work()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id };
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.CheckRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            var any = await users.AnyAsync(query);

            Assert.True(any);
        }

        [Fact]
        public async Task Put_serializedEntity_should_work()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.CheckRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            await users.UpdateByIdAsync(id, Candidate<User, Guid>.Parse(JsonConvert.SerializeObject(user)), query);

            Assert.True(true);
        }

        class OverrideObjectPatcher<T> : ObjectPatcher<T>
            where T : class, new()
        {
            public OverrideObjectPatcher(IPatcherProvider provider) : base(provider, new ReflectionHelper())
            {
            }

            public override object PatchValue(object patchedObject, Type expectedType, JsonObject json)
            {
                return new T();
            }
        }

        [Fact]
        public async Task Put_on_new_entity()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            var users = new RestCollection<User, Guid>(_fixture.UsersRepo, new OverrideObjectPatcher<User>(_fixture.PatcherProvider), _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.CheckRights = false;

            var updated = await users.UpdateByIdAsync(id, Candidate<User, Guid>.Parse(JsonConvert.SerializeObject(user)), query);

            Assert.Equal(new Guid(), updated.Id);
        }

        [Fact]
        public async Task Get_withFilters_shouldWork()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.CheckRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            query = new Query<User>(query, u => u.TwitterUri == new Uri("https://twitter.com"));

            var results = await users.GetAsync(query);

            Assert.Equal(1, results.Count);
        }

        [Fact]
        public void Collection_on_hierarchy_fails()
        {
            Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                var repo = new Repository<Hierarchy>(_fixture.InMemoryStorage, new Mock<IRightExpressionsHelper<Hierarchy>>().Object);
                var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
                var collection = new RestCollection<Hierarchy, int>(repo, new ObjectPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper), instanciator);

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        new BaseClassJsonConverter<Hierarchy>(new InheritanceConfiguration())
                    }
                };
                var candidate = Candidate<Hierarchy, int>.Parse(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
                await collection.CreateAsync(candidate);
            });
        }

        [Fact]
        public async Task BaseClass_Collection_on_hierarchy_works()
        {
            var repo = new Repository<Hierarchy>(_fixture.InMemoryStorage, new Mock<IRightExpressionsHelper<Hierarchy>>().Object);
            var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
            var collection = new RestCollection<Hierarchy, int>(repo, new BaseClassPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper, new InheritanceConfiguration()), instanciator);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                    {
                        new BaseClassJsonConverter<Hierarchy>(new InheritanceConfiguration())
                    }
            };
            var candidate = Candidate<Hierarchy, int>.Parse(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
            await collection.CreateAsync(candidate);
        }
    }
}