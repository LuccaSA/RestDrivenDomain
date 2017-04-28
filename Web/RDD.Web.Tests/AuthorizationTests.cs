using Moq;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Web.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Web.Tests
{
	public class AuthorizationTests
	{
		[Fact]
		public void ShouldThrow500WhenIncorrectAuthorization()
		{
			RDD.Infra.BootStrappers.TestsBootStrapper.ApplicationStart();

			var headers = new NameValueCollection();
			headers.Add("Authorization", "Lucca application=xxx");

			var webContext = new Mock<IWebContext>();
			webContext.Setup(c => c.Headers).Returns(headers);
			var webServices = new Mock<IWebServicesCollection>();

			var service = new HttpAuthenticationService(webContext.Object, webServices.Object);

			Assert.Throws<UnauthorizedException>(() =>
			{
				service.Authenticate();
			});

			headers.Remove("Authorization");
			headers.Add("Authorization", "Lucca application=88888888-333-4444-4444-121212121212");

			Assert.Throws<UnauthorizedException>(() =>
			{
				service.Authenticate();
			});
		}
	}
}
