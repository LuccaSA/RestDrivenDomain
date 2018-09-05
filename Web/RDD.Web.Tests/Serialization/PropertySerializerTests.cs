using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.UrlProviders;
using RDD.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class PropertySerializerTests
    {
        [Fact]
        public void PropertySerializer_should_serialize_url_properly()
        {
            var entity = new User { Id = 1 };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);

            var tree = ExpressionSelectorTree<User>.New(u => u.Url);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.Equal("https://mon.domain.com/api/users/1", json.GetJsonValue("Url"));
        }

        [Fact]
        public void PropertySerializer_should_accept_custom_apiPrefix()
        {
            var entity = new User { Id = 1 };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new PrefixUrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);

            var tree = ExpressionSelectorTree<User>.New(u => u.Url);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.Equal("https://mon.domain.com/api/lol/users/1", json.GetJsonValue("Url"));
        }

        class PrefixUrlProvider : UrlProvider
        {
            public PrefixUrlProvider(IPluralizationService pluralizationService, IHttpContextAccessor httpContextAccessor) : base(pluralizationService, httpContextAccessor)
            {
            }

            protected override string ApiPrefix => "api/lol";
        }

        [Fact]
        public void ValueObject_should_serializeAllProperties()
        {
            var entity = new User
            {
                Id = 1,
                MyValueObject = new MyValueObject
                {
                    Id = 123,
                    Name = "test"
                }
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);

            var tree = ExpressionSelectorTree<User>.New(u => u.MyValueObject);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.True(json.HasJsonValue("MyValueObject.Id"));
            Assert.True(json.HasJsonValue("MyValueObject.Name"));
        }

        [Fact]
        public void MultiplePropertiesOnSubTypeShouldSerialize()
        {
            var entity = new User
            {
                Department = new Department
                {
                    Id = 1,
                    Name = "Department"
                }
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);
            
            var tree = ExpressionSelectorTree<User>.New(u => u.Department.Id, (User u) => u.Department.Name);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.True(json.HasJsonValue("Department.Id"));
            Assert.True(json.HasJsonValue("Department.Name"));
            Assert.True(json.GetJsonValue("Department.Id") == "1");
            Assert.True(json.GetJsonValue("Department.Name") == "Department");
        }

        [Fact]
        public void SubEntityBase_should_serializeIdNameUrl()
        {
            var entity = new User
            {
                Id = 1,
                Department = new Department
                {
                    Id = 2,
                    Name = "Foo",
                    Url = "/api/departements/2"
                }
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);

            var tree = ExpressionSelectorTree<User>.New(u => u.Department);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.True(json.HasJsonValue("Department.Id"));
            Assert.True(json.HasJsonValue("Department.Name"));
            Assert.True(json.HasJsonValue("Department.Url"));
        }

        [Fact]
        public void ListSubTypeShouldSerialize()
        {
            var entity = new Department
            {
                Users = new List<User>
                {
                    new User
                    {
                        Id = 0,
                        Name = "Peter"
                    },
                    new User
                    {
                        Id = 1,
                        Name = "Steven"
                    }
                }
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);
            
            var tree = ExpressionSelectorTree<Department>.New(u => u.Users);
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.True(json.GetJsonValue("Users.0.Name") == "Peter");
            Assert.True(json.GetJsonValue("Users.0.Id") == "0");
            Assert.True(json.GetJsonValue("Users.1.Name") == "Steven");
            Assert.True(json.GetJsonValue("Users.1.Id") == "1");
        }

        [Fact]
        public void ListSubTypeWithPropertySelectorShouldSerialize()
        {
            var entity = new Department
            {
                Users = new List<User>
                {
                    new User
                    {
                        Id = 0,
                        Name = "Peter"
                    },
                    new User
                    {
                        Id = 1,
                        Name = "Steven"
                    }
                }
            };
            var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            httpContextAccessor.HttpContext.Request.Scheme = "https";
            httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
            var urlProvider = new UrlProvider(new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US"))), httpContextAccessor);

            var serializer = new SerializerProvider(new ReflectionProvider(new MemoryCache(new MemoryCacheOptions())), urlProvider);

            var tree = ExpressionSelectorTree<Department>.New(u => u.Users.Select(g => g.Name));
            var json = serializer.ToJson(entity, tree) as JsonObject;

            Assert.True(json.GetJsonValue("Users.0.Name") == "Peter");
            Assert.False(json.HasJsonValue("Users.0.Id"));
            Assert.True(json.GetJsonValue("Users.1.Name") == "Steven");
            Assert.False(json.HasJsonValue("Users.0.Id"));
        }
    }
}