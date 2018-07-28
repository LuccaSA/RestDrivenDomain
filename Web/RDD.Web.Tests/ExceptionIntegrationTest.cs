using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Tests.ServerMock;
using Xunit;

namespace RDD.Web.Tests
{
    public class ExceptionIntegrationTest
    {
        private HttpClient _client;
        private TestServer _server;

        private void SetupServer(Action<IServiceCollection> configureServices = null)
        {
            var host = Startup.BuildWebHost(null);
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
            var repo = new Mock<IRepository<ExchangeRate>>();
            repo.Setup(r => r.GetAsync(It.IsAny<Query<ExchangeRate>>()))
                .Throws(exception);

            SetupServer(service =>
            {
                service.AddScoped(_=> QueryFactoryHelper.NewQueryFactory());
                service.AddScoped(_ => repo.Object);
                service.Configure<ExceptionHttpStatusCodeOption>(options =>
                {
                    options.StatusCodeMapping = e =>
                    {
                        switch (e)
                        {
                            case TestException _:
                                return HttpStatusCode.Ambiguous;
                            default:
                                return null;
                        }
                    };
                });
            });
            var response = await _client.GetAsync("/ExchangeRate/");
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
