using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using RDD.Web.Tests.ServerMock;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            host.ConfigureServices(c => { });
            _server = new TestServer(host);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetOkAsync()
        {
            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Get;
            var response = await _client.GetAsync("/ExchangeRate/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.GetAsync("/ExchangeRate/");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Id = 3, Name = "putted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Put;
            var response = await _client.PutAsync("/ExchangeRate/", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.PutAsync("/ExchangeRate/", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Name = "posted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Post;
            var response = await _client.PostAsync("/ExchangeRate/", content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); //basic route not found

            response = await _client.PostAsync("/ExchangeRate/creation", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); //overriden route found

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.PostAsync("/ExchangeRate/creation", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Id = 4 });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.None;
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/ExchangeRate/") { Content = content });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedHttpVerbs = Domain.Helpers.HttpVerbs.Delete;
            response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/ExchangeRate/") { Content = content });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByIdOkAsync()
        {
            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.Get;
            var response = await _client.GetAsync("/ExchangeRate/23");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.GetAsync("/ExchangeRate/23");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutByIdOkAsync()
        {
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Id = 5, Name = "putted2" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.Put;
            var response = await _client.PutAsync("/ExchangeRate/5", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.None;
            response = await _client.PutAsync("/ExchangeRate/5", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteByIdOkAsync()
        {
            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.None;
            var response = await _client.DeleteAsync("/ExchangeRate/7");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            ExchangeRateController.ConfigurableAllowedByIdHttpVerbs = Domain.Helpers.HttpVerbs.Delete;
            response = await _client.DeleteAsync("/ExchangeRate/7");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}