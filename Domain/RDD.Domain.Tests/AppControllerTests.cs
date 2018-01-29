using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.Storage;
using System;
using System.Collections.Generic;
using System.Text;
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
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollectionWithHardcodedGetById(repo, _execution, _combinationsHolder);
                var controller = new UsersAppController(storage, users);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                var user = await controller.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), query);

                Assert.Equal(3, user.Id);
            }
        }

        [Fact]
        public async Task PutShouldNotCallGetByIdOnTheCollection()
        {
            using (var storage = _newStorage(Guid.NewGuid().ToString()))
            {
                var repo = new Repository<User>(storage, _execution, _combinationsHolder);
                var users = new UsersCollectionWithHardcodedGetById(repo, _execution, _combinationsHolder);
                var controller = new UsersAppController(storage, users);
                var query = new Query<User>();
                query.Options.CheckRights = false;

                await controller.CreateAsync(PostedData.ParseJson(@"{ ""id"": 3 }"), query);

                var user = await controller.UpdateByIdAsync(3, PostedData.ParseJson(@"{ ""name"": ""newName"" }"), query);

                Assert.Equal(3, user.Id);
            }
        }
    }
}
