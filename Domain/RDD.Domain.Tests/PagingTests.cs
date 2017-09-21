using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Storage;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
	public class PagingTests : SingleContextTests
	{
		IRepository<User> _repo;
		IReadOnlyRestCollection<User> _collection;
		IStorageService _storage;

		public PagingTests()
		{
			_storage = _newStorage(Guid.NewGuid().ToString());
			_repo = new GetFreeRepository<User>(_storage, _execution, _combinationsHolder);
			_collection = new UsersCollection(_repo, _execution, _combinationsHolder);
		}

		[Fact]
		public async void Default_Paging_should_be_0_to_10()
		{
			var users = User.GetManyRandomUsers(20);
			_repo.AddRange(users);
			await _storage.SaveChangesAsync();

			var query = new Query<User>();
			var result = await _collection.GetAsync(query);

			Assert.Equal(0, query.Page.Offset);
			Assert.Equal(10, query.Page.Limit);
			Assert.Equal(10, result.Items.Count());
			Assert.Equal(20, result.Count);
		}

		[Fact]
		public void Paging_should_limit_to_1000_result()
		{
			Assert.ThrowsAsync<OutOfRangeException>(async () =>
			{
				var users = User.GetManyRandomUsers(2000);
				_repo.AddRange(users);
				await _storage.SaveChangesAsync();

				var query = new Query<User>() { Page = new Page(0, 1001) };
				var result = await _collection.GetAsync(query);
			});
		}
	}
}
