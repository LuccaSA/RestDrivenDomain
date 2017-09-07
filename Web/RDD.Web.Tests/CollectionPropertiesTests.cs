using Moq;
using RDD.Domain.Contracts;
using RDD.Domain.Models.Convertors;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Web.QueryParsers;
using System.Diagnostics;
using Xunit;

namespace RDD.Web.Tests
{
	public class CollectionPropertiesTests
	{
		[Fact]
		public void Sum_of_id_SHOULD_work_on_collection()
		{
			var repoMock = new Mock<IRepository<User>>();
			var convertorMock = new Mock<IQueryConvertor<User>>();

			var users = new UsersCollection(new Stopwatch(), repoMock.Object, convertorMock.Object);

			var fields = "id,name,collection.sum(id)";

			var result = users.Get(new Query<User> { Fields = new FieldsParser<User>().ParseFields(fields) });

			Assert.Equal(0, result.Count);
		}
	}
}