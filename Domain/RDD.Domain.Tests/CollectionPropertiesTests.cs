using NUnit.Framework;
using RDD.Domain.Contexts;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
    public class CollectionPropertiesTests
    {
		[Test]
		public void Sum_of_id_should_work_on_collection()
		{
			using (var storage = new InMemoryStorageService())
			{
				var execution = new InMemoryExecutionContext();
				var users = new UsersCollection(storage, execution, null);

				var fields = "id,name,collection.sum(id)";

				var result = users.Get(new Query<User> { Fields = Field.Parse<User>(fields) });

				Assert.AreEqual(0, result.Count);
			}
		}
    }
}
