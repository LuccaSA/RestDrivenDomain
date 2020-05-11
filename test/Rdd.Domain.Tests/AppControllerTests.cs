using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Json;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class AppControllerTests : IClassFixture<DefaultFixture>
    {
        private readonly DefaultFixture _fixture;
        private readonly ICandidateParser _parser;

        private class OptionsAccessor : IOptions<MvcNewtonsoftJsonOptions>
        {
            public static MvcNewtonsoftJsonOptions JsonOptions = new MvcNewtonsoftJsonOptions();
            public MvcNewtonsoftJsonOptions Value => JsonOptions;
        }

        public AppControllerTests(DefaultFixture fixture)
        {
            _fixture = fixture;
            _parser = new CandidateParser(new JsonParser(), new OptionsAccessor());
        }

        [Fact]
        public async Task PostShouldNotCallGetByIdOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User>();
            query.Options.ChecksRights = false;
            var id = Guid.NewGuid();
            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            var user = await controller.CreateAsync(candidate, query);

            Assert.Equal(id, user.Id);
        }

        [Fact]
        public async Task PostShouldFailedIfForbidden()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User> { Verb = Helpers.HttpVerbs.Post };
            var id = Guid.NewGuid();
            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            await Assert.ThrowsAsync<ForbiddenException>(() => controller.CreateAsync(candidate, query));
        }

        [Fact]
        public async Task PostShouldWorkIfForbiddenButExplicitelyAllowed()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User> { Verb = Helpers.HttpVerbs.Post };
            query.Options.ChecksRights = false;
            var id = Guid.NewGuid();
            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            var user = await controller.CreateAsync(candidate, query);

            Assert.Equal(id, user.Id);
        }

        [Fact]
        public async Task PostShouldNotCallGetByIdsOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User> { Verb = Helpers.HttpVerbs.Post };
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id2}"" }}");

            var result = (await controller.CreateAsync(new List<ICandidate<User, Guid>> { candidate1, candidate2 }, query)).ToList();

            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public async Task PostOnCollectionShouldFailedIfForbidden()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User> { Verb = Helpers.HttpVerbs.Post };
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id2}"" }}");

            await Assert.ThrowsAsync<ForbiddenException>(() => controller.CreateAsync(new List<ICandidate<User, Guid>> { candidate1, candidate2 }, query));
        }

        [Fact]
        public async Task PostOnCollectionShouldWorkIfForbiddenButExplicitelyAllowed()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User>();
            query.Options.ChecksRights = false;
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id2}"" }}");

            var result = (await controller.CreateAsync(new List<ICandidate<User, Guid>> { candidate1, candidate2 }, query)).ToList();

            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public async Task CreateAsyncShouldNotCallGetByIdsOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var result = (await controller.CreateAsync(new List<User> { new User { Id = id1 }, new User { Id = id2 } })).ToList();

            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public async Task CreateAsyncCollectionShouldFailedIfForbidden()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);

            await Assert.ThrowsAsync<ForbiddenException>(() => controller.CreateAsync(new List<User> { new User { Id = Guid.NewGuid() }, new User { Id = Guid.NewGuid() } }));
        }

        [Fact]
        public async Task CreateAsyncCollectionShouldWorkIfForbiddenButExplicitelyAllowed()
        {
            var repo = new Repository<User>(_fixture.InMemoryStorage, new ClosedRightExpressionsHelper<User>(), _fixture.IncludeApplicator);
            var users = new UsersCollectionWithHardcodedGetById(repo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var result = (await controller.CreateAsync(new List<User> { new User { Id = id1 }, new User { Id = id2 } }, false)).ToList();

            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public async Task PutShouldNotCallGetByIdOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User>();
            query.Options.ChecksRights = false;
            var id = Guid.NewGuid();
            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            await controller.CreateAsync(candidate, query);

            candidate = _parser.Parse<User, Guid>(@"{ ""name"": ""newName"" }");

            var user = await controller.UpdateByIdAsync(id, candidate, query);

            Assert.Equal(id, user.Id);
        }
    }
}