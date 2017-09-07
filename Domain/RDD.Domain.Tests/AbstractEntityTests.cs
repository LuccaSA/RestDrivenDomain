using Microsoft.EntityFrameworkCore;
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

	internal class ConcreteClassThreeCollection : ReadOnlyRestCollection<ConcreteClassThree, int>
	{
		public ConcreteClassThreeCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, combinationsHolder, asyncStorage) { }
	}

	public class AbstractEntityTests
	{
		[Fact]
		public async void NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseInMemoryDatabase(databaseName: "NonAbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called")
				.Options;

			using (var storage = new EFStorageService(new DataContext(options)))
			{
				var execution = new ExecutionContextMock();

				storage.Add(new ConcreteClassThree());
				storage.Add(new ConcreteClassThree());

				await storage.CommitAsync();

				var collection = new ConcreteClassThreeCollection(storage, execution, null, () => storage);

				var result = await collection.GetAllAsync();

				Assert.Equal(2, result.Count());
			}
		}

		[Fact]
		public async void AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called()
		{
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseInMemoryDatabase(databaseName: "AbstractCollection_SHOULD_return_all_entities_WHEN_GetAll_is_called")
				.Options;

			using (var storage = new EFStorageService(new DataContext(options)))
			{
				var execution = new ExecutionContextMock();

				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassOne());
				storage.Add(new ConcreteClassTwo());

				await storage.CommitAsync();

				var collection = new AbstractClassCollection(storage, execution, null);

				var result = await collection.GetAllAsync();

				Assert.Equal(3, result.Count());
			}
		}
	}
}
