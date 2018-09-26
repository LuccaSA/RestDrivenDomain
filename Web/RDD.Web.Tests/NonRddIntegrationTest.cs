using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using RDD.Web.Tests.ServerMock;
using Xunit;

namespace RDD.Web.Tests
{
    public class NonRddIntegrationTest
    {
        private readonly HttpClient _client;
        private readonly TestServer _server;

        public NonRddIntegrationTest()
        {
            var host = Startup.BuildWebHost(null);
            host.ConfigureServices(c => { });
            _server = new TestServer(host);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetOkAsync()
        {
            var response = await _client.GetAsync("/NonRdd/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}