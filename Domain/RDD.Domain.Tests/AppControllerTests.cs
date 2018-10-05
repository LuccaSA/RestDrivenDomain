﻿using RDD.Domain.Json;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
    public class AppControllerTests : IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;

        public AppControllerTests(DefaultFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PostShouldNotCallGetByIdOnTheCollection()
        {
            var users = new UsersCollectionWithHardcodedGetById(_fixture.UsersRepo, _fixture.PatcherProvider, _fixture.Instanciator);
            var controller = new UsersAppController(_fixture.InMemoryStorage, users);
            var query = new Query<User>();
            query.Options.CheckRights = false;
            var id = Guid.NewGuid();
            var candidate = new CandidateParser(new JsonParser()).Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

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
            var candidate1 = new CandidateParser(new JsonParser()).Parse<User, Guid>($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = new CandidateParser(new JsonParser()).Parse<User, Guid>($@"{{ ""id"": ""{id2}"" }}");

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
            var parser = new CandidateParser(new JsonParser());
            var candidate = parser.Parse<User, Guid>($@"{{ ""id"": ""{id}"" }}");

            await controller.CreateAsync(candidate, query);

            candidate = parser.Parse<User, Guid>(@"{ ""name"": ""newName"" }");

            var user = await controller.UpdateByIdAsync(id, candidate, query);

            Assert.Equal(id, user.Id);
        }
    }
}