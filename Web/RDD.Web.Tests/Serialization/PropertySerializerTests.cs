using Microsoft.AspNetCore.Http;
using RDD.Domain.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Tests.Models;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class PropertySerializerTests
    {
        [Fact]
        public void PropertySerializer_should_serialize_url_properly()
        {
            var entity = new User
            {
                Id = 1
            };

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(httpContextAccessor);

            var serializer = new EntitySerializer(urlProvider);

            var propertySelector = new PropertySelector<User>();
            propertySelector.Add(u => u.Url);
            var dico = serializer.SerializeEntity(entity, propertySelector);

            Assert.Equal("https://mon.domain.com/api/users/1", dico["Url"]);
        }

        [Fact]
        public void PropertySerializer_should_accept_custom_apiPrefix()
        {
            var entity = new User
            {
                Id = 1
            };

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new PrefixUrlProvider(httpContextAccessor);

            var serializer = new EntitySerializer(urlProvider);

            var propertySelector = new PropertySelector<User>();
            propertySelector.Add(u => u.Url);
            var dico = serializer.SerializeEntity(entity, propertySelector);

            Assert.Equal("https://mon.domain.com/api/lol/users/1", dico["Url"]);
        }

        class PrefixUrlProvider : UrlProvider
        {
            public PrefixUrlProvider(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
            { }

            public override string GetApiPrefix() => "api/lol";
        }
    }
}
