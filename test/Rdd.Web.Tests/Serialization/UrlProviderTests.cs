using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rdd.Application;
using Rdd.Web.Controllers;
using Rdd.Web.Serialization.UrlProviders;
using Rdd.Web.Tests.Models;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Rdd.Web.Tests.Serialization
{
    [Collection("automapper")]
    public class UrlProviderTests
    {
        private readonly HttpContextAccessor httpContextAccessor;

        public UrlProviderTests()
        {
            httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
        }

        private IActionDescriptorCollectionProvider GetProvider<TController>(string template)
            where TController : ControllerBase => GetProvider(typeof(TController), template);

        private IActionDescriptorCollectionProvider GetProvider(Type type, string template)
        {
            var mock = new Mock<IActionDescriptorCollectionProvider>();
            mock.Setup(a => a.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                new ControllerActionDescriptor
                {
                    ActionName = "GetByIdAsync",
                    ControllerTypeInfo = type.GetTypeInfo(),
                    AttributeRouteInfo = new Microsoft.AspNetCore.Mvc.Routing.AttributeRouteInfo { Template = template }
                }
            }, 1));

            return mock.Object;
        }
        [Theory]
        [InlineData(typeof(ReadOnlyWebController<User, int>))]
        [InlineData(typeof(ReadOnlyWebController<IReadOnlyAppController<User, int>, User, int>))]
        [InlineData(typeof(WebController<User, int>))]
        [InlineData(typeof(WebController<IAppController<User, int>, User, int>))]
        public void GetEntityUrl_should_return_valid_url(Type type)
        {
            var entity = new User
            {
                Id = 1
            };

            var urlProvider = new UrlProvider(GetProvider(type, "api/users/{id}"), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/users/1", result.ToString());
        }

        [Fact]
        public void GetEntityUrl_should_return_valid_url_diffeent_template()
        {
            var entity = new User
            {
                Id = 1
            };

            var urlProvider = new UrlProvider(GetProvider<ReadOnlyWebController<User, int>>("api/users/{userId}"), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/users/1", result.ToString());
        }

        [Fact]
        public void GetEntityUrl_should_return_valid_url_with_no_inheritance()
        {
            var entity = new UserTest();

            var urlProvider = new UrlProvider(GetProvider<ReadOnlyWebController<User, int>>("api/users/{id}"), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/users/10", result.ToString());
        }

        [Fact]
        public void GetEntityUrl_should_return_valid_url_with_inheritance_if_needed()
        {
            var entity = new UserTest();

            var urlProvider = new UserTestUrlProvider(GetProvider<ReadOnlyWebController<User, int>>("api/users/{id}"), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/specific/url/10", result.ToString());
        }

        [Fact]
        public void UnknownObject_get_no_url()
        {
            var entity = new Department();

            var urlProvider = new UserTestUrlProvider(GetProvider<ReadOnlyWebController<User, int>>("api/users/{id}"), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Null(result);
        }

        [Fact]
        public void RealUrls()
        {
            var host = HostBuilder.FromStartup<Startup>().Build(); 
            var urlProvider = host.Services.GetRequiredService<IUrlProvider>();

            var entity = new ExchangeRate { Id = 123 };
            var result = urlProvider.GetEntityApiUri(entity);
            Assert.Equal("https://mon.domain.com/ExchangeRates/123", result.ToString());
        }

        [Fact]
        public void RealUrlsAspnet22()
        {
            var host = HostBuilder.FromStartup<StartupMvc22>().Build();
            var urlProvider = host.Services.GetRequiredService<IUrlProvider>();

            var entity = new ExchangeRate { Id = 123 };
            var result = urlProvider.GetEntityApiUri(entity);
            Assert.Equal("https://mon.domain.com/ExchangeRates/123", result.ToString());
        }

        private class UserTest : User
        {
            public UserTest()
            {
                Id = 10;
            }
        }

        private class UserTestUrlProvider : UrlProvider
        {
            public UserTestUrlProvider(IActionDescriptorCollectionProvider provider, IHttpContextAccessor httpContextAccessor)
                : base(provider, httpContextAccessor)
            {
            }

            protected override Dictionary<Type, string> CompileUrls()
            {
                var result = base.CompileUrls();
                result[typeof(UserTest)] = "specific/url/{0}";
                return result;
            }
        }
    }
}