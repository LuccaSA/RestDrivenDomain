using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Infra.Storage;
using System;
using Xunit;

namespace RDD.Domain.Tests
{
	public class QueryTests : SingleContextTests
	{
		IRepository<User> _repo;
		IReadOnlyRestCollection<User> _collection;
		IStorageService _storage;

		public QueryTests()
		{
			_storage = _newStorage(Guid.NewGuid().ToString());
			_repo = new Repository<User>(_storage, _execution, _combinationsHolder);
			_collection = new UsersCollection(_repo, _execution, _combinationsHolder);
		}

		[Fact]
		public void Cloning_query_should_not_clone_verb()
		{
			var query = new Query<User> { Verb = HttpVerb.PUT };
			var result = new Query<User>(query);

			Assert.Equal(HttpVerb.GET, result.Verb);
		}

		[Fact]
		public void Cloning_query_should_not_clone_stopwatch()
		{
			var query = new Query<User> { Verb = HttpVerb.PUT };
			var result = new Query<User>(query);

			Assert.NotEqual(query.Watch, result.Watch);
		}
	}
}
