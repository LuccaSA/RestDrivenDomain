using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
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
        private readonly DefaultFixture _fixture;

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
                var unitOfWork = new UnitOfWork(context);
                var storage = new EFStorageService(context);
                var repo = new UsersRepository(storage, _fixture.RightsService);
                var collection = new UsersCollection(repo, _fixture.PatcherProvider, _fixture.Instanciator);
                var users = User.GetManyRandomUsers(20).OrderBy(u => u.Id).ToList();
                await repo.AddRangeAsync(users, new Query<User> { Verb = HttpVerbs.Post });
                await unitOfWork.SaveChangesAsync();

                var request = HttpVerbs.Get.NewRequest(("mail", "aaa3@example.com"));
                var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);
                var result = (await collection.GetAsync(query)).Items;

                Assert.Single(result);
                Assert.Equal("John Doe 3", result.FirstOrDefault().Name);
            });
        }

        //ce test vérifie que la manière dont est convertie les filtres menant à un left join + test de nullité de clé fonctionne jusque dans EF
        [Theory]
        [InlineData(nameof(Parent.OptionalChild), "notequal,null")]
        [InlineData(nameof(Parent.OptionalChild), "null")]
        [InlineData(nameof(Parent.OptionalChild) + "." + nameof(OptionalChild.Name), "notequal,null")]
        [InlineData(nameof(Parent.OptionalChild) + "." + nameof(OptionalChild.Name), "null")]
        [InlineData(nameof(Parent.OptionalChild) + "." + nameof(OptionalChild.Name), "value")]
        public async void LeftJoinShouldWork(string key, string value)
        {
            await RunCodeInsideIsolatedDatabaseAsync(async (context) =>
            {
                var unitOfWork = new UnitOfWork(context);
                context.Add(new Parent { OptionalChild = new OptionalChild { Name = "value" } });
                context.Add(new Parent());
                await unitOfWork.SaveChangesAsync();

                var request = HttpVerbs.Get.NewRequest((key, value));
                var query = QueryParserHelper.GetQueryParser<Parent>().Parse(request, true);

                var storage = new EFStorageService(context);
                var repo = new OpenRepository<Parent>(storage, null);
                var collection = new RestCollection<Parent, int>(repo, new ObjectPatcher<Parent>(_fixture.PatcherProvider, new ReflectionHelper()), new DefaultInstanciator<Parent>());
                var result = (await collection.GetAsync(query)).Items;

                Assert.Single(result);
            });
        }
    }
}