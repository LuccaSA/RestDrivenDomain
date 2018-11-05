using Rdd.Domain.Helpers;
using Rdd.Domain.Tests;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Infra.Tests;
using System.Linq;
using Xunit;

namespace Rdd.Web.Tests
{
    public class IntegrationTests : DatabaseTest, IClassFixture<DefaultFixture>
    {
        private DefaultFixture _fixture;

        public IntegrationTests(DefaultFixture fixture)
        {
            _fixture = fixture;
        }

        //ce test vérifie que la manière dont est convertie les filtres sur les mailsaddress fonctionne jusque dans EF
        [Fact]
        public async void MailAdressFiltersShouldWork()
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context, null);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
                var users = User.GetManyRandomUsers(20).OrderBy(u => u.Id).ToList();
                repo.AddRange(users);
                await unitOfWork.SaveChangesAsync();

                var request = HttpVerbs.Get.NewRequest(("mail", "aaa3@example.com"));
                var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);
                var result = (await collection.GetAsync(query)).Items;

                Assert.Single(result);
                Assert.Equal("John Doe 3", result.FirstOrDefault().Name);
            }, true);
        }
    }
}