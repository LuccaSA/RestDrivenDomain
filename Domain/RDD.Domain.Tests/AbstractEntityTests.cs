using RDD.Domain.Tests.Models;
using RDD.Infra.Services;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
	public class AbstractEntityTests
	{
		[Fact]
		public void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			using (var storage = new InMemoryStorageService())
			{
				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassTwo());

				Assert.Equal(3, storage.Set<AbstractClass>().Count());
			}
		}
	}
}
