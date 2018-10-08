using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Web.Tests
{
    public class ApiExplorerTests
    {
        [Fact]
        public void ApiExplorer()
        {
            var host = Startup.BuildWebHost(null).Build();
            var apiExplorer = host.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            //les items contient tous 3 les controllers (userweb, exchangerate, et openexchangerate)
            //SEUL openexchangerate affiche un attribut  [ApiExplorerSettings(IgnoreApi = false)]
            //qui le dévoile à l'api explorer

            Assert.Single(apiExplorer.ApiDescriptionGroups.Items);

            var elementsTests = new List<Predicate<ApiDescription>>
            {
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "GET" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "POST" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "PUT" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "DELETE" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "GET" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "PUT" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "DELETE" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
            };

            //les 7 routes: get, getbyId, post, put, putbyid, delete, deletebyid
            Assert.Equal(elementsTests.Count, apiExplorer.ApiDescriptionGroups.Items[0].Items.Count);

            foreach (var elementTest in elementsTests)
            {
                Assert.Contains(apiExplorer.ApiDescriptionGroups.Items[0].Items, elementTest);
            }
        }
    }
}
