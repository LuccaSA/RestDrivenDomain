using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Infra.Tests
{
    public class CollectionTests : DatabaseTest, IClassFixture<DefaultFixture>
    {
        private readonly DefaultFixture _fixture;

        public CollectionTests(DefaultFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Guids_ordering_in_sql_should_work_properly()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
                var users = User.GetManyRandomUsers(20).OrderBy(u => u.Id).ToList();
                await repo.AddRangeAsync(users, new Query<User>{Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                var sequence = users.Select(i => i.Id).ToList();

                var result = (await collection.GetAsync(new Query<User>())).Items.ToList();

                Assert.Equal(sequence, result.Select(i => i.Id).ToList());
            });
        }

        [Fact]
        public async Task NullableGuids_ordering_in_sql_should_work_properly()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var users = User.GetManyRandomUsers(20).OrderBy(u => u.FriendId).ThenBy(u => u.Id).ToList();
                await repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                var sequence = users.Select(i => i.Id).ToList();

                var query = new Query<User> { OrderBys = new List<OrderBy<User>> { OrderBy<User>.Ascending(u => u.FriendId), OrderBy<User>.Ascending(u => u.Id) } };
                var result = (await collection.GetAsync(query)).Items.ToList();

                Assert.Equal(sequence, result.Select(i => i.Id).ToList());
            });
        }

        [Fact]
        public async Task DeleteRangeWorks()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var users = User.GetManyRandomUsers(20).ToList();
                await repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                storage.RemoveRange(users);
                await unitOfWork.SaveChangesAsync();

                var result = (await collection.GetAsync(new Query<User>())).Items.ToList();

                Assert.Empty(result);
            });
        }


        [Fact]
        public async Task Any_ShouldReturnTrue_WhenExistingUser()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                //Arrange
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var user = User.GetManyRandomUsers(1).First();
                await repo.AddAsync(user, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                //Act
                var exist = await collection.AnyAsync(new Query<User>(u => u.Id == user.Id));

                //Assert
                Assert.True(exist);
            });
        }

        [Fact]
        public async Task Any_ShouldReturnTrue_WhenExistingUser_AndRightCheckedOnOpenRepository()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                //Arrange
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var user = User.GetManyRandomUsers(1).First();
                await repo.AddAsync(user, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                //Act
                var query = new Query<User>(u => u.Id == user.Id);
                query.Options.ChecksRights = true;
                var exist = await collection.AnyAsync(query);

                //Assert
                Assert.True(exist);
            });
        }

        [Fact]
        public async Task Any_ShouldReturnFalse_WhenNoExistingUser()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                //Arrange
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var user = User.GetManyRandomUsers(1).First();
                await repo.AddAsync(user, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                
                //Act
                var exist = await collection.AnyAsync(new Query<User>(u => Guid.NewGuid() == user.Id));

                //Assert
                Assert.False(exist);
            });
        }

        [Fact]
        public async Task ExecuteScriptDoesNotThrow()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var result = await new EFStorageService(context).ExecuteScriptAsync($"Use [{DbName}] SELECT * FROM dbo.[User]");
                Assert.Equal(-1, result);
            });
        }

        [Fact]
        public async Task DiscardChangesWork()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var users = User.GetManyRandomUsers(3).ToList();
                await repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                users[0].Name = "new value";
                repo.Remove(users[2]);

                var newUser = new User();
                await repo.AddAsync(newUser, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });

                storage.DiscardChanges<User>(null);//does not fail
                storage.DiscardChanges(users[0]);//modified
                storage.DiscardChanges(users[1]);//unmodified
                storage.DiscardChanges(users[2]);//removed
                storage.DiscardChanges(new User());//unattached
                storage.DiscardChanges(newUser);//added
                await unitOfWork.SaveChangesAsync();

                var result = (await collection.GetAsync(new Query<User>())).Items.ToList();

                Assert.Equal(3, result.Count);
                Assert.NotEqual("new value", result[0].Name);
            });
        }

        [Fact]
        public async Task IncludeIntersectionWork()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var dpts = new List<Department> { new Department(), new Department() };
                storage.AddRange(dpts);
                await unitOfWork.SaveChangesAsync();

                var users = User.GetManyRandomUsers(1, dpts).ToList();
                await repo.AddRangeAsync(users, new Query<User> { Verb = Domain.Helpers.HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                using (var newContext = GetContext())
                {
                    var newStorage = new EFStorageService(newContext);
                    var newRepo = new UsersRepository(newStorage, _fixture.RightsService);

                    var query = new Query<User>();
                    var noDpts = (await newRepo.GetAsync(query)).ToList();
                    Assert.Null(noDpts[0].Department);

                    query = new Query<User> { Fields = new ExpressionParser().ParseTree<User>("department[id,name],id,name") };
                    var withDpts = (await newRepo.GetAsync(query)).ToList();
                    Assert.NotNull(noDpts[0].Department);
                }
            });
        }
    }
}