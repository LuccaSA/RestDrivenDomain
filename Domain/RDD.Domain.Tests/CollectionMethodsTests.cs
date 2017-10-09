using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using RDD.Infra.Storage;
using System;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionMethodsTests : SingleContextTests
	{
		[Fact]
		public async void GetById_SHOULD_throw_exception_WHEN_id_does_not_exist()
		{
			using (var storage = _newStorage(Guid.NewGuid().ToString()))
			{
				var user = new User { Id = 1 };
				var repo = new OpenRepository<User>(storage, _execution, _combinationsHolder);
				var users = new UsersCollection(repo, _execution, _combinationsHolder);

				await users.CreateAsync(user);

				await storage.SaveChangesAsync();

				await Assert.ThrowsAsync<NotFoundException>(() => users.GetByIdAsync(0));
			}
		}

		[Fact]
		public async void TryGetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
		{
			using (var storage = _newStorage(Guid.NewGuid().ToString()))
			{
				var user = new User { Id = 2 };
				var repo = new Repository<User>(storage, _execution, _combinationsHolder);
				var users = new UsersCollection(repo, _execution, _combinationsHolder);

				await users.CreateAsync(user);

				await storage.SaveChangesAsync();

				Assert.Null(await users.TryGetByIdAsync(0));
			}
		}

		[Fact]
		public async void Put_SHOULD_throw_notfound_exception_WHEN_unexisting_entity_()
		{
			using (var storage = _newStorage(Guid.NewGuid().ToString()))
			{
				_execution.curPrincipal = new WebService { Id = 1, AppOperations = new HashSet<int>() { 1 } };

				var mock = new Mock<ICombinationsHolder>();
				mock.Setup(h => h.Combinations)
					.Returns(new HashSet<Combination>() {
						new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerb.POST },
						new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerb.PUT }
					});
				var combinationsHolder = mock.Object;

				var user = new User { Id = 3 };
				var repo = new Repository<User>(storage, _execution, combinationsHolder);
				var users = new UsersCollection(repo, _execution, combinationsHolder);
                var app = new UsersAppController(storage, users);

                await app.CreateAsync(PostedData.ParseJSON(@"{ ""id"": 3 }"), new Query<User>());

                await Assert.ThrowsAsync<NotFoundException>(() => app.UpdateAsync(0, PostedData.ParseJSON(@"{ ""name"": ""new name"" }"), new Query<User>()));
			}
		}
	}
}
