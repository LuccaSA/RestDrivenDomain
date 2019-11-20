using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Models;
using Rdd.Web.Querying;
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
        private readonly ICandidateParser _parser;

        private class OptionsAccessor : IOptions<MvcNewtonsoftJsonOptions>
        {
            public MvcNewtonsoftJsonOptions Value { get; }

            public OptionsAccessor()
            {
                Value = new MvcNewtonsoftJsonOptions();
                Value.SerializerSettings.Converters = new List<JsonConverter>
                {
                    new BaseClassJsonConverter<Hierarchy>(new InheritanceConfiguration())
                };
            }
        }

        public CollectionMethodsTests()
        {
            _fixture = new DefaultFixture();
            _parser = new CandidateParser(new JsonParser(), new OptionsAccessor());
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
            rightService.Setup(s => s.GetFilterAsync(It.IsAny<Query<User>>())).Returns(Task.FromResult(trueFilter));

            var id = Guid.NewGuid();
            var repo = new Repository<User>(_fixture.InMemoryStorage, rightService.Object);
            var users = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var app = new UsersAppController(_fixture.InMemoryStorage, users);
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");
            var candidate2 = _parser.Parse<User, Guid>(@"{ ""name"": ""new name"" }");

            await app.CreateAsync(candidate1, new Query<User>());
            await app.UpdateByIdAsync(Guid.NewGuid(), candidate2, new Query<User>());
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsNotOverridenAndEntityHasAParameterlessConstructor()
        {
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.ChecksRights = false;

            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{Guid.NewGuid()}"" }}");
            await users.CreateAsync(candidate, query);
        }

        private class InstanciatorImplementation : IInstanciator<UserWithParameters>
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
            var repo = new Repository<UserWithParameters>(_fixture.InMemoryStorage, new OpenRightExpressionsHelper<UserWithParameters>());
            var users = new UsersCollectionWithParameters(repo, _fixture.PatcherProvider, new InstanciatorImplementation());
            var query = new Query<UserWithParameters>();
            query.Options.ChecksRights = false;

            var candidate = _parser.Parse<UserWithParameters, int>(@"{ ""id"": 3, ""name"": ""John"" }");
            var result = await users.CreateAsync(candidate, query);
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
            query.Options.ChecksRights = false;

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
            query.Options.ChecksRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            var candidate = _parser.Parse<User, Guid>(JsonConvert.SerializeObject(user));
            await users.UpdateByIdAsync(id, candidate, query);
            Assert.True(true);
        }

        private class OverrideObjectPatcher<T> : ObjectPatcher<T>
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
            query.Options.ChecksRights = false;

            var updated = await users.UpdateByIdAsync(id, _parser.Parse<User, Guid>(JsonConvert.SerializeObject(user)), query);

            Assert.Equal(new Guid(), updated.Id);
        }

        [Fact]
        public async Task Get_withFilters_shouldWork()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new Query<User>();
            query.Options.ChecksRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            query = new Query<User>(query, u => u.TwitterUri == new Uri("https://twitter.com"));

            var results = await users.GetAsync(query);

            Assert.Equal(1, results.Count);
        }

        [Fact]
        public async Task Collection_on_hierarchy_fails()
        {
            await Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                var repo = new Repository<Hierarchy>(_fixture.InMemoryStorage, new Mock<IRightExpressionsHelper<Hierarchy>>().Object);
                var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
                var collection = new RestCollection<Hierarchy, int>(repo, new ObjectPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper), instanciator);

                var candidate = _parser.Parse<Hierarchy, int>(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
                await collection.CreateAsync(candidate, new Query<Hierarchy> { Verb = Helpers.HttpVerbs.Post });
            });
        }

        [Fact]
        public async Task BaseClass_Collection_on_hierarchy_works()
        {
            var repo = new Repository<Hierarchy>(_fixture.InMemoryStorage, new OpenRightExpressionsHelper<Hierarchy>());
            var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
            var collection = new RestCollection<Hierarchy, int>(repo, new BaseClassPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper, new InheritanceConfiguration()), instanciator);

            var candidate = _parser.Parse<Hierarchy, int>(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
            await collection.CreateAsync(candidate, new Query<Hierarchy> { Verb = Helpers.HttpVerbs.Post });
        }
    }
}