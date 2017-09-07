using Moq;
using RDD.Domain.Contracts;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Convertors;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries;
using RDD.Domain.Tests.Models;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionMethodsTests
	{
		[Fact]
		public void GetById_SHOULD_throw_exception_WHEN_id_does_not_exist()
		{
			var repoMock = new Mock<IRepository<User>>();
			repoMock.Setup(r => r.Get(It.IsAny<IStorageQuery<User>>())).Returns(new List<User>());

			var convertorMock = new Mock<IQueryConvertor<User>>();
			convertorMock.Setup(c => c.Convert(It.IsAny<Query<User>>(), It.IsAny<Stopwatch>())).Returns(new Mock<IStorageQuery<User>>().Object);

			var users = new UsersCollection(new Stopwatch(), repoMock.Object, convertorMock.Object);

			Assert.Throws<NotFoundException>(() => users.GetById(0));
		}

		[Fact]
		public void TryGetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
		{
			var repoMock = new Mock<IRepository<User>>();
			repoMock.Setup(r => r.Get(It.IsAny<IStorageQuery<User>>())).Returns(new List<User>());

			var convertorMock = new Mock<IQueryConvertor<User>>();
			convertorMock.Setup(c => c.Convert(It.IsAny<Query<User>>(), It.IsAny<Stopwatch>())).Returns(new Mock<IStorageQuery<User>>().Object);

			var users = new UsersCollection(new Stopwatch(), repoMock.Object, convertorMock.Object);
			Assert.Null(users.TryGetById(0));
		}

		[Fact]
		public void Put_SHOULD_throw_notfound_exception_WHEN_unexisting_entity_()
		{
			var repoMock = new Mock<IRepository<User>>();
			repoMock.Setup(r => r.Get(It.IsAny<IStorageQuery<User>>())).Returns(new List<User>());

			var storageQueryMock = new Mock<IStorageQuery<User>>();

			var convertorMock = new Mock<IQueryConvertor<User>>();
			convertorMock.Setup(c => c.Convert(It.IsAny<Query<User>>(), It.IsAny<Stopwatch>())).Returns(storageQueryMock.Object);

			var users = new UsersCollection(new Stopwatch(), repoMock.Object, convertorMock.Object);
			Assert.Throws<NotFoundException>(() => users.Update(0, new { name = "new name" }));
		}
	}
}