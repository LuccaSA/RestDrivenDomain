using Moq;
using RDD.Domain;
using RDD.Infra.Contexts;
using RDD.Web.Contexts;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Web.Tests
{
	public class AsyncServiceTests
	{
		private readonly IAsyncService _asyncService;

		private Mock<ICollection> _mock { get; set; }

		public AsyncServiceTests()
		{
			_asyncService = new AsyncService(new InMemoryWebContext());

			_mock = new Mock<ICollection>();
			_mock.Setup(m => m.GetEnumerator()).Verifiable();
		}

		private void CallVerifiableMockMethod()
		{
			_mock.Object.GetEnumerator();
		}

		[Fact]
		public async Task AsyncService_ShouldBeTestable_WhenCallingContinueAsync()
		{
			await _asyncService.ContinueAsync(() => CallVerifiableMockMethod());

			_mock.Verify(m => m.GetEnumerator(), Times.Once());
		}

		[Fact]
		public void AsyncService_ShouldBeTestable_WhenCallingRunInParallel()
		{
			var list = new List<int> { 1, 2, 3 };

			_asyncService.RunInParallel<int>(list, (number) => CallVerifiableMockMethod());

			_mock.Verify(m => m.GetEnumerator(), Times.Exactly(3));
		}
	}
}
