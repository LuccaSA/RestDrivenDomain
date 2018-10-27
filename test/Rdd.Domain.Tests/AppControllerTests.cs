using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rdd.Domain.Json;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Web.Models;
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
        private DefaultFixture _fixture;
        private readonly ICandidateParser _parser;

        private class OptionsAccessor : IOptions<MvcJsonOptions>
        {
            public static MvcJsonOptions JsonOptions = new MvcJsonOptions();
            public MvcJsonOptions Value => JsonOptions;
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
            query.Options.CheckRights = false;
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
            var query = new Query<User>();
            query.Options.CheckRights = false;
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var candidate1 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id2}"" }}");

            var result = (await controller.CreateAsync(new List<ICandidate<User, Guid>> { candidate1, candidate2 }, query)).ToList();

            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public async Task PutShouldNotCallGetByIdOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User>();
            query.Options.CheckRights = false;
            var id = Guid.NewGuid();
            var candidate = _parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            await controller.CreateAsync(candidate, query);

            candidate = _parser.Parse<User, Guid>(@"{ ""name"": ""newName"" }");

            var user = await controller.UpdateByIdAsync(id, candidate, query);

            Assert.Equal(id, user.Id);
        }
    }
}