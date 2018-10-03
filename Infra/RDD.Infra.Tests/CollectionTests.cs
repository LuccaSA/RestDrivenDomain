using RDD.Domain.Models.Querying;
using RDD.Domain.Tests;
using RDD.Domain.Tests.Models;
using RDD.Infra.Storage;
using System.Linq;
using Xunit;

namespace RDD.Infra.Tests
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
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
                var users = User.GetManyRandomUsers(20).OrderBy(u => u.Id).ToList();
                var firstUser = users.ElementAt(0);
                var tenthUser = users.ElementAt(9);
                var lastUser = users.ElementAt(19);
                repo.AddRange(users);
                await storage.SaveChangesAsync();
                var result = (await collection.GetAsync(new Query<User>())).Items.ToList();

                var firstItem = result.ElementAt(0);
                var tenthItem = result.ElementAt(9);
                var lastItem = result.ElementAt(19);

                Assert.Equal(firstUser, firstItem);
                Assert.Equal(tenthUser, tenthItem);
                Assert.Equal(lastUser, lastItem);
            });
        }
    }
}
