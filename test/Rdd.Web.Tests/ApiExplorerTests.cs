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
            var host = HostBuilder.FromStartup<Startup>().Build();

            var apiExplorer = host.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            //les items contient toutes les routes de tous les controllers (userweb, exchangerate, openexchangerate, ExchangeRate3Controller)
            //mais dans un seul groupe
            Assert.Single(apiExplorer.ApiDescriptionGroups.Items);

            var routeTests = new List<Predicate<ApiDescription>>
            {
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "GET" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "POST" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "PUT" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName && d.HttpMethod == "DELETE" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "GET" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "PUT" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
                d => d.RelativePath == ExchangeRate2Controller.RouteName + "/{id}" && d.HttpMethod == "DELETE" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),

                d => d.RelativePath == "api/ExchangeRate3" && d.HttpMethod == "GET" && d.ParameterDescriptions.Count == 0,
                d => d.RelativePath == "api/ExchangeRate3/{id}" && d.HttpMethod == "GET" && d.ParameterDescriptions.All(p => p.Name == "id" && p.Type == typeof(int)),
            };

            var routes = apiExplorer.ApiDescriptionGroups.Items.First().Items;

            //les 7 routes: get, getbyId, post, put, putbyid, delete, deletebyid open
            //les 2 routes sans override de route
            Assert.Equal(routeTests.Count, routes.Count);

            foreach (var elementTest in routeTests)
            {
                Assert.Contains(routes, elementTest);
            }
        }
    }
}
