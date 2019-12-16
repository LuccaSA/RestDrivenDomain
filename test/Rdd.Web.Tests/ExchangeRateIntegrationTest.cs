using System;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Rdd.Web.Tests.ServerMock;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Domain.Helpers;
using Xunit;

namespace Rdd.Web.Tests
{
    [Collection("automapper")]
    public class ExchangeRateIntegrationTest
    {
        private IWebHostBuilder _host;

        public ExchangeRateIntegrationTest()
        {
            _host = HostBuilder.FromStartup<Startup>();
        }

        private HttpClient CreateClient(Func<IServiceProvider, ForceVerb> allowed)
        {
            _host.ConfigureServices(services =>
            {
                services.AddScoped(allowed);
            });
            var server = new TestServer(_host);
            return server.CreateClient();
        }

        [Fact]
        public async Task GetOkAsync()
        {
            HttpVerbs verb = HttpVerbs.Get;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var response = await client.GetAsync("/ExchangeRates/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            verb = HttpVerbs.None;
            response = await client.GetAsync("/ExchangeRates/");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task PutOkAsync()
        {
            HttpVerbs verb = HttpVerbs.Put;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var serialized = JsonConvert.SerializeObject(new { Id = 3, Name = "putted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");
             
            var response = await client.PutAsync("/ExchangeRates/", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            verb = HttpVerbs.None;
            response = await client.PutAsync("/ExchangeRates/", content);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task PostOkAsync()
        {
            HttpVerbs verb = HttpVerbs.Post;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var serialized = JsonConvert.SerializeObject(new { Name = "posted" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            verb = HttpVerbs.Post;
            var response = await client.PostAsync("/ExchangeRates/", content);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode); //basic route not found

            response = await client.PostAsync("/ExchangeRates/creation", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); //overriden route found

            verb = HttpVerbs.None;
            response = await client.PostAsync("/ExchangeRates/creation", content);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task DeleteOkAsync()
        {
            HttpVerbs verb = HttpVerbs.None;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var serialized = JsonConvert.SerializeObject(new ExchangeRate { Id = 4 });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/ExchangeRates/") { Content = content });
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

            verb = HttpVerbs.Delete;
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/ExchangeRates/") { Content = content });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByIdOkAsync()
        {
            HttpVerbs verb = HttpVerbs.Get;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var response = await client.GetAsync("/ExchangeRates/23");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var response404 = await client.GetAsync("/ExchangeRates/2300");
            Assert.Equal(HttpStatusCode.NotFound, response404.StatusCode);

            verb = HttpVerbs.None;
            response = await client.GetAsync("/ExchangeRates/23");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task IgnoredAndBadFilters()
        {
            HttpVerbs verb = HttpVerbs.Get;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb }); 
            var response = await client.GetAsync("/ExchangeRates/customRoute?pipo=12&pipo2=23");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PutByIdOkAsync()
        {
            HttpVerbs verb = HttpVerbs.Put;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });
            var serialized = JsonConvert.SerializeObject(new { Id = 5, Name = "putted2" });
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            var response = await client.PutAsync("/ExchangeRates/5", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            verb = HttpVerbs.None;
            response = await client.PutAsync("/ExchangeRates/5", content);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task DeleteByIdOkAsync()
        {
            HttpVerbs verb = HttpVerbs.None;
            var client = CreateClient(s => new ForceVerb { HttpVerbs = verb });

            var response = await client.DeleteAsync("/ExchangeRates/7");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);

            verb = HttpVerbs.Delete;
            response = await client.DeleteAsync("/ExchangeRates/7");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}