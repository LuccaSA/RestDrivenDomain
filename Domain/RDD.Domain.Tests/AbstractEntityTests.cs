using Microsoft.EntityFrameworkCore;
using RDD.Domain.Mocks;
using RDD.Domain.Models;
using RDD.Domain.Storage;
using RDD.Domain.Tests.Models;
using RDD.Infra.Services;
using System;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
	internal class AbstractClassCollection : ReadOnlyRestCollection<AbstractClass, int>
	{
		public AbstractClassCollection(IRepository<AbstractClass> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
			: base(repository, execution, combinationsHolder) { }
	}

	internal class ConcreteClassThreeCollection : ReadOnlyRestCollection<ConcreteClassThree, int>
	{
		public ConcreteClassThreeCollection(IRepository<ConcreteClassThree> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
			: base(repository, execution, combinationsHolder) { }
	}

	public class AbstractEntityTests
	{
		[Fact]
		public async void NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			var execution = new ExecutionContextMock();
			var combinationHolder = new CombinationsHolderMock();
			var storage = new InMemoryStorageService();
			var repo = new OpenRepository<ConcreteClassThree>(storage, execution, combinationHolder);

			repo.Add(new ConcreteClassThree());
			repo.Add(new ConcreteClassThree());

			var collection = new ConcreteClassThreeCollection(repo, execution, null);

			var result = await collection.GetAllAsync();

			Assert.Equal(2, result.Count());
		}

		[Fact]
		public async void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			var execution = new ExecutionContextMock();
			var combinationHolder = new CombinationsHolderMock();
			var storage = new InMemoryStorageService();
			var repo = new OpenRepository<AbstractClass>(storage, execution, combinationHolder);

			repo.Add(new ConcreteClassOne());
			repo.Add(new ConcreteClassOne());
			repo.Add(new ConcreteClassTwo());

			var collection = new AbstractClassCollection(repo, execution, combinationHolder);

			var result = await collection.GetAllAsync();

			Assert.Equal(3, result.Count());
		}
	}
}
