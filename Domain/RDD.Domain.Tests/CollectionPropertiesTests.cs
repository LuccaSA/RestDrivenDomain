using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionPropertiesTests
	{
		[Fact]
		public void Sum_of_id_SHOULD_work_on_collection()
		{
			using (var storage = new InMemoryStorageService())
			{
				var execution = new InMemoryExecutionContext();
				var users = new UsersCollection(storage, execution, null, null);

				var fields = "id,name,collection.sum(id)";

				var result = users.Get(new Query<User> { Fields = Field.Parse<User>(fields) });

				Assert.Equal(0, result.Count);
			}
		}
	}
}
