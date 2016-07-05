using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.BootStrappers;
using RDD.Infra.Services;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Infra.Tests
{
	[TestClass]
	public class AsyncServiceTests
	{
		private readonly IAsyncService _asyncService;

		private Mock<ICollection> _mock { get; set; }

		public AsyncServiceTests()
		{
			TestsBootStrapper.ApplicationStart();
			
			_asyncService = new AsyncService();
		}

		[TestInitialize]
		public void Init()
		{
			TestsBootStrapper.ApplicationBeginRequest();

			_mock = new Mock<ICollection>();
			_mock.Setup(m => m.GetEnumerator()).Verifiable();
		}

		private void CallVerifiableMockMethod()
		{
			_mock.Object.GetEnumerator();
		}

		[TestMethod]
		public async Task AsyncService_ShouldBeTestable_WhenCallingContinueAsync()
		{
			await _asyncService.ContinueAsync(() => CallVerifiableMockMethod());

			_mock.Verify(m => m.GetEnumerator(), Times.Once());
		}

		[TestMethod]
		public void AsyncService_ShouldBeTestable_WhenCallingRunInParallel()
		{
			var list = new List<int> { 1, 2, 3 };

			_asyncService.RunInParallel<int>(list, (number) => CallVerifiableMockMethod());

			_mock.Verify(m => m.GetEnumerator(), Times.Exactly(3));
		}
	}
}
