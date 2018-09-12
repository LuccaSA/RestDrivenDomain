using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.Storage;
using RDD.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
    public class AppControllerTests : SingleContextTests
    {
        [Fact]
        public async Task PostShouldNotCallGetByIdOnTheCollection()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollectionWithHardcodedGetById(repo, _patcherProvider, Instanciator);
                var controller = new UsersAppController(storage, users);
                var query = new Query<User>();
                query.Options.CheckRights = false;
                var candidate = Candidate<User, int>.Parse(@"{ ""id"": 3 }");

                var user = await controller.CreateAsync(candidate, query);

                Assert.Equal(3, user.Id);
            }
        }

        [Fact]
        public async Task PostShouldNotCallGetByIdsOnTheCollection()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollectionWithHardcodedGetById(repo, _patcherProvider, Instanciator);
                var controller = new UsersAppController(storage, users);
                var query = new Query<User>();
                query.Options.CheckRights = false;
                var candidate1 = Candidate<User, int>.Parse(@"{ ""id"": 3 }");
                var candidate2 = Candidate<User, int>.Parse(@"{ ""id"": 4 }");

                var result = (await controller.CreateAsync(new List<Candidate<User, int>> { candidate1, candidate2 }, query)).ToList();

                Assert.Equal(3, result[0].Id);
                Assert.Equal(4, result[1].Id);
            }
        }

        [Fact]
        public async Task PutShouldNotCallGetByIdOnTheCollection()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _rightsService);
                var users = new UsersCollectionWithHardcodedGetById(repo, _patcherProvider, Instanciator);
                var controller = new UsersAppController(storage, users);
                var query = new Query<User>();
                query.Options.CheckRights = false;
                var candidate = Candidate<User, int>.Parse(@"{ ""id"": 3 }");

                await controller.CreateAsync(candidate, query);

                candidate = Candidate<User, int>.Parse(@"{ ""name"": ""newName"" }");

                var user = await controller.UpdateByIdAsync(3, candidate, query);

                Assert.Equal(3, user.Id);
            }
        }
    }
}
