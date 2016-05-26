using NUnit.Framework;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.BootStrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
	[TestFixture]
	public class CollectionMethodsTests : SingleContextTests
	{
		[Test]
		public void GetById_should_throw_exception()
		{
			TestsBootStrapper.ApplicationBeginRequest();

			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, _newStorage);

			users.Create(user);

			Assert.Throws<NotFoundException>(() => users.GetById(0));
		}

		[Test]
		public void TryGetById_should_not_throw_exception_and_return_null()
		{
			TestsBootStrapper.ApplicationBeginRequest();

			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, _newStorage);

			users.Create(user);

			Assert.IsNull(users.TryGetById(0));
		}
	}
}
