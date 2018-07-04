using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RDD.Domain.Helpers;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
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
            }
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

            var propertySelector = new PropertySelector<User>(u => u.MyValueObject);
            var json = serializer.ToJson(entity, new Web.Serialization.Options.SerializationOption { Selectors = new[] { propertySelector } }) as JsonObject;

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

            var json = serializer.ToJson(entity, new Web.Serialization.Options.SerializationOption
            {
                Selectors = new[] {
                new PropertySelector<User>(u => u.Department.Id),
                new PropertySelector<User>(u => u.Department.Name) }
            }) as JsonObject;

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

            var json = serializer.ToJson(entity, new Web.Serialization.Options.SerializationOption { Selectors = new[] { new PropertySelector<User>(u => u.Department) } }) as JsonObject;

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

            var json = serializer.ToJson(entity, new Web.Serialization.Options.SerializationOption { Selectors = new[] { new PropertySelector<Department>(u => u.Users) } }) as JsonObject;

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

            var json = serializer.ToJson(entity, new Web.Serialization.Options.SerializationOption { Selectors = new[] { new PropertySelector<Department>(u => u.Users.Select(g => g.Name)) } }) as JsonObject;

            Assert.True(json.GetJsonValue("Users.0.Name") == "Peter");
            Assert.False(json.HasJsonValue("Users.0.Id"));
            Assert.True(json.GetJsonValue("Users.1.Name") == "Steven");
            Assert.False(json.HasJsonValue("Users.0.Id"));
        }
    }
}