using Microsoft.AspNetCore.TestHost;
using Rdd.Web.Tests.ServerMock;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    [Collection("automapper")]
    public class DTOCatIntegrationTest
    {
        private readonly HttpClient _client;
        private readonly TestServer _server;

        public DTOCatIntegrationTest(AutomapperFixture fixture)
        {
            var host = HostBuilder.FromStartup<Startup>(null);
            host.ConfigureServices(c => { });
            _server = new TestServer(host);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetOkAsync()
        {
            CatsController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Get;
            var response = await _client.GetAsync("/Cats");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            CatsController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.GetAsync("/Cats");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task GetOkComplexAsync()
        {
            CatsController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Get;
            var response = await _client.GetAsync("/Cats?fields=nickname&orderby=age,asc&nickname=kitty");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByIdOkAsync()
        {
            CatsController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Get;
            var response = await _client.GetAsync("/Cats/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var response404 = await _client.GetAsync("/Cats/123");
            Assert.Equal(HttpStatusCode.NotFound, response404.StatusCode);

            CatsController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.GetAsync("/Cats/1");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
    }
}