using Microsoft.EntityFrameworkCore;
using RDD.Domain.Tests.Models;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using System;

namespace RDD.Domain.Tests.Templates
{
	public class SingleContextTests
	{
		protected IDependencyInjectionResolver _resolver;
		protected IStorageService _storage;
		protected IExecutionContext _execution;
		protected Func<IStorageService> _newStorage;

		public SingleContextTests()
		{
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseInMemoryDatabase(databaseName: "SingleContextTests")
				.Options;

			_newStorage = () => new EFStorageService(new DataContext(options));
			_storage = _newStorage();
			_execution = new InMemoryExecutionContext();
		}
	}
}
