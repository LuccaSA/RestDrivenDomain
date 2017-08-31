using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Tests.Models;
using RDD.Infra.Services;
using System;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
	internal class AbstractClassCollection : ReadOnlyRestCollection<AbstractClass, int>
	{
		public AbstractClassCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, combinationsHolder, asyncStorage) { }
	}

	public class AbstractEntityTests
	{
		[Fact]
		public void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			using (var storage = new InMemoryStorageService())
			{
				var execution = new ExecutionContextMock();

				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassTwo());

				var collection = new AbstractClassCollection(storage, execution, null);

				var result = collection.GetAll();

				Assert.Equal(3, result.Count());
			}
		}
	}
}
