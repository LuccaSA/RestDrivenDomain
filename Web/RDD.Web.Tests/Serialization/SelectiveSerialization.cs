using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RDD.Domain;
using RDD.Web.Tests.Models;
using RDD.Web.Tests.ServerMock;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class SelectiveSerialization
    {
        public SelectiveSerialization()
        {
            SetupServer(service =>
            {
                service.AddScoped(_ => QueryFactoryHelper.NewQueryFactory());
                service.AddScoped<UsersController>();
                service.AddScoped<IRepository<User>, UserRepository>();
            });
        }

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
        public async Task SelectiveFieldsDefault()
        {
            var response = await _client.GetAsync("/users/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var found = (await GetData<User>(response)).FirstOrDefault();

            Assert.NotNull(found);
            Assert.Null(found.Department);
            Assert.Null(found.MyValueObject);
        }

        [Fact]
        public async Task SelectiveSubFields()
        {
            var response = await _client.GetAsync("/users/?fields=id,name,department[name],myValueObject[id]");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var found = (await GetData<User>(response)).FirstOrDefault();

            Assert.NotNull(found);
            Assert.NotNull(found.Department);
            Assert.NotNull(found.Department.Name);
            Assert.Equal(0,found.Department.Id);

            Assert.NotNull(found.MyValueObject);
            Assert.NotEqual(0,found.MyValueObject.Id);
            Assert.Null(found.MyValueObject.Name);
        }

        [Fact]
        public async Task SelectiveSubsFieldsIEnumerable()
        {
            var response = await _client.GetAsync("/users/?fields=id,name,files.fileDescriptor[id,fileName],myValueObject[id]");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var found = (await GetData<User>(response)).FirstOrDefault();

            Assert.NotNull(found);
            Assert.Null(found.Department);

            Assert.NotNull(found.Files);
            Assert.NotNull(found.Files.FirstOrDefault());
            Assert.NotNull(found.Files.FirstOrDefault().FileDescriptor); 
            Assert.NotNull(found.Files.FirstOrDefault().FileDescriptor.FileName);

            Assert.NotNull(found.MyValueObject);
            Assert.NotEqual(0, found.MyValueObject.Id);
            Assert.Null(found.MyValueObject.Name);
        }

        private async Task<IEnumerable<T>> GetData<T>(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync();
            var extracted = JsonConvert.DeserializeObject<Root<T>>(data);
            return extracted.Data.Items;
        }
    }


    public class Root<T>
    {
        public Header Header { get; set; }
        public Data<T> Data { get; set; }
    }

    public class Header
    {
        public string Generated { get; set; }
        public string Principal { get; set; }
        public Paging Paging { get; set; }
    }

    public class Paging
    {
        public int Count { get; set; }
        public int Offset { get; set; }
        public string previous { get; set; }
        public string next { get; set; }
    }

    public class Data<T>
    {
        public T[] Items { get; set; }
    }
}