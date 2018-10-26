using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Infra.Tests
{
    public class CollectionTests : DatabaseTest, IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;

        public CollectionTests(DefaultFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async void Guids_ordering_in_sql_should_work_properly()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
                var users = User.GetManyRandomUsers(20).OrderBy(u => u.Id).ToList();
                repo.AddRange(users);
                await unitOfWork.SaveChangesAsync();

                var sequence = users.Select(i => i.Id).ToList();

                var result = (await collection.GetAsync(new Query<User>())).Items.ToList();

                Assert.Equal(sequence, result.Select(i => i.Id).ToList());
            });
        }

        [Fact]
        public async void NullableGuids_ordering_in_sql_should_work_properly()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);

                var users = User.GetManyRandomUsers(20).OrderBy(u => u.FriendId).ThenBy(u => u.Id).ToList();
                repo.AddRange(users);
                await unitOfWork.SaveChangesAsync();

                var sequence = users.Select(i => i.Id).ToList();

                var query = new Query<User> { OrderBys = new List<OrderBy<User>> { OrderBy<User>.Ascending(u => u.FriendId), OrderBy<User>.Ascending(u => u.Id) } };
                var result = (await collection.GetAsync(query)).Items.ToList();

                Assert.Equal(sequence, result.Select(i => i.Id).ToList());
            });
        }
    }
}