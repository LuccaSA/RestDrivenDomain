using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using Rdd.Web.Models;
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
            var candidate = Candidate<User, Guid>.Parse($@"{{ ""id"": ""{id}"" }}");

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
            var candidate1 = Candidate<User, Guid>.Parse($@"{{ ""id"": ""{id1}"" }}");
            var candidate2 = Candidate<User, Guid>.Parse($@"{{ ""id"": ""{id2}"" }}");

            var result = (await controller.CreateAsync(new List<Candidate<User, Guid>> { candidate1, candidate2 }, query)).ToList();

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
            var candidate = Candidate<User, Guid>.Parse($@"{{ ""id"": ""{id}"" }}");

            await controller.CreateAsync(candidate, query);

            candidate = Candidate<User, Guid>.Parse($@"{{ ""name"": ""newName"" }}");

            var user = await controller.UpdateByIdAsync(id, candidate, query);

            Assert.Equal(id, user.Id);
        }
    }
}
