using System;
using Microsoft.AspNetCore.Http;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Serialization.UrlProviders;
using RDD.Web.Tests.Models;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class UrlProviderTests
    {
        private HttpContextAccessor httpContextAccessor;

        public UrlProviderTests()
        {
            httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
        }
        [Fact]
        public void GetEntityUrl_should_return_valid_url()
        {
            var entity = new User
            {
                Id = 1
            };

            var urlProvider = new UrlProvider(new PluralizationService(), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/users/1", result.ToString());
        }

        [Fact]
        public void GetEntityUrl_should_return_valid_url_with_no_inheritance()
        {
            var entity = new UserTest();

            var urlProvider = new UrlProvider(new PluralizationService(), httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/usertests/10", result.ToString());
        }

        [Fact]
        public void GetEntityUrl_should_return_valid_url_with_inheritance_if_needed()
        {
            var entity = new UserTest();

            var urlProvider = new UserTestUrlProvider(httpContextAccessor);
            var result = urlProvider.GetEntityApiUri(entity);

            Assert.Equal("https://mon.domain.com/api/users/10", result.ToString());
        }

        private class UserTest : User
        {
            public override int Id { get => 10; }
        }

        private class UserTestUrlProvider : UrlProvider
        {
            public UserTestUrlProvider(IHttpContextAccessor httpContextAccessor)
                : base(new PluralizationService(), httpContextAccessor) { }

            public override string GetApiControllerName(Type workingType)
            {
                if (workingType == typeof(UserTest))
                {
                    return base.GetApiControllerName(typeof(User));
                }

                return base.GetApiControllerName(workingType);
            }
        }
    }
}