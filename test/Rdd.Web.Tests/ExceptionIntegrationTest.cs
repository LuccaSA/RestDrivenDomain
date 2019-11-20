using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Helpers;
using Rdd.Web.Tests.ServerMock;
using Xunit;

namespace Rdd.Web.Tests
{
    [Collection("automapper")]
    public class ExceptionIntegrationTest
    {
        private HttpClient _client;
        private TestServer _server;

        private void SetupServer(Action<IServiceCollection> configureServices = null)
        {
            var host = HostBuilder.FromStartup<Startup>();
            if (configureServices != null)
            {
                host.ConfigureServices(configureServices);
            }
            _server = new TestServer(host);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task MissingRouteShouldReturn404()
        {
            SetupServer();

            var response = await _client.GetAsync("/unknownRoute/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(ExceptionCases))]
        public async Task HttpCodeExceptionOption(Exception exception, HttpStatusCode expected)
        {
            var repo = new Mock<IRepository<ExchangeRate2>>();
            repo.Setup(r => r.GetAsync(It.IsAny<Query<ExchangeRate2>>()))
                .Throws(exception);

            SetupServer(service =>
            {
                service.AddScoped(_ => repo.Object);
                service.Configure<ExceptionHttpStatusCodeOption>(options => options.StatusCodeMapping = e => e switch
                {
                    TestException _ => HttpStatusCode.Ambiguous,
                    _ => null,
                });
            });
            var response = await _client.GetAsync("/OpenExchangeRates/");
            Assert.Equal(expected, response.StatusCode);
        }

        public static IEnumerable<object[]> ExceptionCases()
        {
            yield return new object[] { new TestException() , HttpStatusCode.Ambiguous };
            yield return new object[] { new UnauthorizedException("") , HttpStatusCode.Unauthorized };
            yield return new object[] { new TestExceptionWithStatus(), HttpStatusCode.PaymentRequired};
        }

        public class TestException : Exception { }

        public class TestExceptionWithStatus : Exception, IStatusCodeException
        {
            public HttpStatusCode StatusCode => HttpStatusCode.PaymentRequired;
        }
    }
}
