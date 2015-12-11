using NUnit.Framework;
using RDD.Domain.Models.Querying;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web.Tests
{
    public class CollectionPropertiesTests
    {
		[Test]
		public void Count_should_be_zero_when_empy_collection()
		{
			using (var storage = new InMemoryStorageService())
			{
				var execution = new InMemoryExecutionContext();
				var users = new UsersCollection(storage, execution, null);

				var fields = "id,name,collection.count";

				var result = users.Get(new Query<User> { Fields = Field.Parse<User>(fields) });

				Assert.AreEqual(0, result.Count);
			}
		}
    }
}
