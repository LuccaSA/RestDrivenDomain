using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using RDD.Web.Tests.ServerMock;
using Xunit;

namespace RDD.Web.Tests
{
    public class ExchangeRateIntegrationTest
    {
        private readonly HttpClient _client;
        private readonly TestServer _server;

        public ExchangeRateIntegrationTest()
        {
            var host = Startup.BuildWebHost(null);
            host.ConfigureServices(c =>
            {

            });

            _server = new TestServer(host);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetOkAsync()
        {
            var response = await _client.GetAsync("/ExchangeRate/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByIdOkAsync()
        {
            var response = await _client.GetAsync("/ExchangeRate/23");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PutOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Id = 3, Name = "putted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/ExchangeRate/", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Name = "posted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/ExchangeRate/", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOkAsync()
        {
            var response = await _client.DeleteAsync("/ExchangeRate/23");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}