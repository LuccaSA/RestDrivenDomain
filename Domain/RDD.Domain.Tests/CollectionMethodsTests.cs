using Moq;
using RDD.Domain.Contexts;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using RDD.Infra.BootStrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionMethodsTests : SingleContextTests
	{
		[Fact]
		public void GetById_SHOULD_throw_exception_WHEN_id_does_not_exist()
		{
			TestsBootStrapper.ApplicationBeginRequest();

			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, _newStorage);

			users.Create(user);

			Assert.Throws<NotFoundException>(() => users.GetById(0));
		}

		[Fact]
		public void TryGetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
		{
			TestsBootStrapper.ApplicationBeginRequest();

			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, _newStorage);

			users.Create(user);

			Assert.Null(users.TryGetById(0));
		}

		[Fact]
		public void Put_SHOULD_throw_notfound_exception_WHEN_unexisting_entity_()
		{
			TestsBootStrapper.ApplicationBeginRequest();
			_execution.curPrincipal = new WebService { Id = 1, AppOperations = new HashSet<int>() { 1 } };
			_resolver.Register<ICombinationsHolder>(() =>
			{
				var mock = new Mock<ICombinationsHolder>();
				mock.Setup(h => h.Combinations)
				.Returns(new HashSet<Combination>() {
					new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerb.PUT }
				});
				return mock.Object;
			});

			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, _newStorage);

			users.Create(user);

			Assert.Throws<NotFoundException>(() => users.Update(0, new { name = "new name" }));
		}
	}
}
