using Microsoft.AspNetCore.Http;
using RDD.Domain.Helpers;
using RDD.Web.Serialization;
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

            var propertySelector = new PropertySelector<User>(u => u.Url);
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

            var propertySelector = new PropertySelector<User>(u => u.Url);
            var dico = serializer.SerializeEntity(entity, propertySelector);

            Assert.Equal("https://mon.domain.com/api/lol/users/1", dico["Url"]);
        }

        class PrefixUrlProvider : UrlProvider
        {
            public PrefixUrlProvider(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
            { }

            protected override string ApiPrefix => "api/lol";
        }

        [Fact]
        public void ValueObject_should_serializeAllProperties()
        {
            var user = new User
            {
                Id = 1,
                MyValueObject = new MyValueObject
                {
                    Id = 123,
                    Name = "test"
                }
            };

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlProvider = new UrlProvider(httpContextAccessor);
            var serializer = new EntitySerializer(urlProvider);

            var result = serializer.SerializeEntity(user, new PropertySelector<User>(u => u.MyValueObject));

            Assert.True(result.ContainsKey("MyValueObject"));

            var myValueObject = (Dictionary<string, object>)result["MyValueObject"];

            Assert.True(myValueObject.ContainsKey("Id"));
        }

        [Fact]
        public void MultiplePropertiesOnSubTypeShouldSerialize()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlProvider = new UrlProvider(httpContextAccessor);
            var serializer = new EntitySerializer(urlProvider);

            var user = new User
            {
                Department = new Department
                {
                    Id = 1,
                    Name = "Department"
                }
            };

            var result = serializer.SerializeEntity(user, new HashSet<PropertySelector> {
                new PropertySelector<User>(u => u.Department.Id),
                new PropertySelector<User>(u => u.Department.Name)
            });

            Assert.True(result.ContainsKey("Department"));

            var department = (Dictionary<string, object>)result["Department"];

            Assert.True(department.ContainsKey("Id"));
            Assert.True(department.ContainsKey("Name"));

            Assert.Equal(1, department["Id"]);
            Assert.Equal("Department", department["Name"]);
        }

        [Fact]
        public void SubEntityBase_should_serializeIdNameUrl()
        {
            var user = new User
            {
                Id = 1,
                Department = new Department
                {
                    Id = 2,
                    Name = "Foo",
                    Url = "/api/departements/2"
                }
            };

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlProvider = new UrlProvider(httpContextAccessor);
            var serializer = new EntitySerializer(urlProvider);

            var result = serializer.SerializeEntity(user, new PropertySelector<User>(u => u.Department));

            Assert.True(result.ContainsKey("Department"));

            var myValueObject = (Dictionary<string, object>)result["Department"];

            Assert.True(myValueObject.ContainsKey("Id"));
        }

        [Fact]
        public void ListSubTypeShouldSerialize()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlProvider = new UrlProvider(httpContextAccessor);
            var serializer = new EntitySerializer(urlProvider);
            
            var department = new Department
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

            var result = serializer.SerializeEntity(department, new HashSet<PropertySelector> {
                new PropertySelector<Department>(d => d.Users)
            });

            Assert.True(result.ContainsKey("Users"));
            var users = (List<Dictionary<string, object>>)result["Users"];
            Assert.Equal(2, users.Count);

            var user0 = users[0];
            var user1 = users[1];

            Assert.True(user0.ContainsKey("Name"));
            Assert.Equal("Peter", user0["Name"]);
            Assert.True(user0.ContainsKey("Id"));
            Assert.Equal(0, user0["Id"]);

            Assert.True(user1.ContainsKey("Name"));
            Assert.Equal("Steven", user1["Name"]);
            Assert.True(user1.ContainsKey("Id"));
            Assert.Equal(1, user1["Id"]);
        }

        [Fact]
        public void ListSubTypeWithPropertySelectorShouldSerialize()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlProvider = new UrlProvider(httpContextAccessor);
            var serializer = new EntitySerializer(urlProvider);

            var department = new Department
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

            var result = serializer.SerializeEntity(department, new HashSet<PropertySelector> {
                new PropertySelector<Department>(d => d.Users.Select(u => u.Name))
            });

            Assert.True(result.ContainsKey("Users"));
            var users = (List<Dictionary<string, object>>)result["Users"];
            Assert.Equal(2, users.Count);

            var user0 = users[0];
            var user1 = users[1];

            Assert.True(user0.ContainsKey("Name"));
            Assert.Equal("Peter", user0["Name"]);
            Assert.False(user0.ContainsKey("Id"));

            Assert.True(user1.ContainsKey("Name"));
            Assert.Equal("Steven", user1["Name"]);
            Assert.False(user1.ContainsKey("Id"));
        }
    }
}
