using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDD.Domain.Tests.Models;
using RDD.Domain.Models;
using RDD.Infra.Services;
using RDD.Domain.Contexts;
using Moq;
using Xunit;
using RDD.Domain.Mocks;

namespace RDD.Domain.Tests
{
	internal class AbstractClassCollection : ReadOnlyRestCollection<AbstractClass, int>
	{
		public AbstractClassCollection(IStorageService storage, IExecutionContext execution, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, asyncStorage) { }
	}

	public class AbstractEntityTests
	{
		[Fact]
		public void AbstractCollectionShouldWorkAsExpected()
		{
			using (var storage = new InMemoryStorageService())
			{
				var execution = new ExecutionContextMock();

				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassTwo());

				var collection = new AbstractClassCollection(storage, execution);

				var result = collection.GetAll();

				Assert.Equal(3, result.Count());
			}
		}
	}
}
