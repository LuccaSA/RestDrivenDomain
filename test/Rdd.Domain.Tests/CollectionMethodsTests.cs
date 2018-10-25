using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Rights;
using Rdd.Infra.Storage;
using Rdd.Infra.Web.Models;
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

        private class OptionsAccessor : IOptions<MvcJsonOptions>
        {
            public MvcJsonOptions Value { get; }

            public OptionsAccessor()
            {
                Value = new MvcJsonOptions();
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
            var users = new OpenRepository<User, Guid>(_fixture.InMemoryStorage, _fixture.RightsService, new HttpQuery<User, Guid>());

            Assert.Null(await users.GetAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Put_SHOULD_NOT_throw_notfound_exception_WHEN_unexisting_entity_()
        {
            Expression<Func<User, bool>> trueFilter = t => true;
            var rightService = new Mock<IRightExpressionsHelper<User>>();
            rightService.Setup(s => s.GetFilter(It.IsAny<Query<User>>())).Returns(trueFilter);

            var id = Guid.NewGuid(); 
            var repo = new Repository<User, Guid>(_fixture.InMemoryStorage, rightService.Object, new HttpQuery<User, Guid>());
            var users = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var app = new UsersAppController(_fixture.InMemoryStorage, users);
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");
            var candidate2 = _parser.Parse<User, Guid>(@"{ ""name"": ""new name"" }");

            await app.CreateAsync(candidate1);
            await app.UpdateByIdAsync(Guid.NewGuid(), candidate2);
        }

        [Fact]
        public async Task Post_SHOULD_work_WHEN_InstantiateEntityIsNotOverridenAndEntityHasAParameterlessConstructor()
        {
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);

            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{Guid.NewGuid()}"" }}");
            await users.CreateAsync(candidate);
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
            var query = new HttpQuery<UserWithParameters, int>();
            query.Options.CheckRights = false;
            var repo = new Repository<UserWithParameters, int>(_fixture.InMemoryStorage, new OpenRightExpressionsHelper<UserWithParameters>(), query);
            var users = new UsersCollectionWithParameters(repo, _fixture.PatcherProvider, new InstanciatorImplementation());

            var candidate = _parser.Parse<UserWithParameters, int>(@"{ ""id"": 3, ""name"": ""John"" }");
            var result = await users.CreateAsync(candidate);
            Assert.Equal(3, result.Id);
            Assert.Equal("John", result.Name);
        }

        [Fact]
        public async Task Put_serializedEntity_should_work()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            var users = new UsersCollection(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var query = new HttpQuery<User, Guid>();
            query.Options.CheckRights = false;

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            var candidate = _parser.Parse<User, Guid>(JsonConvert.SerializeObject(user));
            await users.UpdateByIdAsync(id, candidate);
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
            var query = new HttpQuery<User, Guid>();
            query.Options.CheckRights = false;

            var updated = await users.UpdateByIdAsync(id, _parser.Parse<User, Guid>(JsonConvert.SerializeObject(user)));

            Assert.Equal(new Guid(), updated.Id);
        }

        [Fact]
        public async Task Get_withFilters_shouldWork()
        {
            var id = Guid.NewGuid();
            var query = new HttpQuery<User, Guid>(u => u.TwitterUri == new Uri("https://twitter.com"));
            query.Options.CheckRights = false;
            var user = new User { Id = id, Name = "Name", Salary = 1, TwitterUri = new Uri("https://twitter.com") };
            var users = new OpenRepository<User, Guid>(_fixture.InMemoryStorage, _fixture.RightsService, query);

            _fixture.InMemoryStorage.Add(user);
            await _fixture.InMemoryStorage.SaveChangesAsync();

            var results = await users.GetAsync();

            Assert.Equal(1, results.Count);
        }

        [Fact]
        public void Collection_on_hierarchy_fails()
        {
            Assert.ThrowsAsync<BadRequestException>(async () =>
            {
                var repo = new Repository<Hierarchy, int>(_fixture.InMemoryStorage, new Mock<IRightExpressionsHelper<Hierarchy>>().Object, new HttpQuery<Hierarchy, int>());
                var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
                var collection = new RestCollection<Hierarchy, int>(repo, new ObjectPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper), instanciator);

                var candidate = _parser.Parse<Hierarchy, int>(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
                await collection.CreateAsync(candidate);
            });
        }

        [Fact]
        public async Task BaseClass_Collection_on_hierarchy_works()
        {
            var repo = new Repository<Hierarchy, int>(_fixture.InMemoryStorage, new Mock<IRightExpressionsHelper<Hierarchy>>().Object, new HttpQuery<Hierarchy, int>());
            var instanciator = new BaseClassInstanciator<Hierarchy>(new InheritanceConfiguration());
            var collection = new RestCollection<Hierarchy, int>(repo, new BaseClassPatcher<Hierarchy>(_fixture.PatcherProvider, _fixture.ReflectionHelper, new InheritanceConfiguration()), instanciator);

            var candidate = _parser.Parse<Hierarchy, int>(@"{ ""type"":""super"", ""superProperty"": ""lol"" }");
            await collection.CreateAsync(candidate);
        }
    }
}