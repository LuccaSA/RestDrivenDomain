using RDD.Domain.Contexts;
using RDD.Infra.BootStrappers;
using RDD.Infra.Contexts;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Templates
{
	public class SingleContextTests
	{
		protected IStorageService _storage;
		protected IExecutionContext _execution;
		protected Func<IStorageService> _newStorage;

		public SingleContextTests()
		{
			_newStorage = () => new InMemoryStorageService();
			_storage = _newStorage();
			_execution = new InMemoryExecutionContext();

			TestsBootStrapper.ApplicationStart();
		}
	}
}
