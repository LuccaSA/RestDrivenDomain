using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Models.Querying;
using RDD.Domain.Storage;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using RDD.Web.Querying;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionPropertiesTests : SingleContextTests
	{
		[Fact]
		public async void Sum_of_id_SHOULD_work_on_collection()
		{
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseInMemoryDatabase(databaseName: "Sum_of_id_SHOULD_work_on_collection")
				.Options;

			using (var storage = new EFStorageService(new DataContext(options)))
			{
				var repo = new GetFreeRepository<User>(storage, _execution, _combinationsHolder);
				var users = new UsersCollection(repo, _execution, _combinationsHolder);

				var fields = "id,name,collection.sum(id)";

				var result = await users.GetAsync(new Query<User> { Fields = new FieldsParser().ParseFields<User>(fields) });

				Assert.Equal(0, result.Count);
			}
		}
	}
}
