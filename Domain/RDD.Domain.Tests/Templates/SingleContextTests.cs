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
			_newStorage = () => new InMemoryStorageService();
			_storage = _newStorage();
			_execution = new InMemoryExecutionContext();
		}
	}
}
