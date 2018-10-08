using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests.Serialization
{
    public partial class FieldsTests
    {
        [Fact]
        public async Task should_serialize_url_properly()
        {
            var entity = new User { Id = 1 };
            var fields = new ExpressionParser().ParseTree<User>("url");

            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""url"":""http://www.example.org/""}"), json);
        }

        [Fact]
        public async Task ValueObject_should_serializeAllProperties()
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

            var fields = ExpressionTree<User>.New(u => u.MyValueObject);
            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""myValueObject"":{""id"":123,""name"":""test"",""user"":null}}"), json);
        }

        [Fact]
        public async Task ValueObject_should_serializeAllPropertiesButRespectDefault()
        {
            var entity = new User
            {
                Id = 1,
                MyValueObject = new MyValueObject
                {
                    Id = 2,
                    Name = "a",
                    User = new User()
                }
            };

            var fields = ExpressionTree<User>.New(u => u.MyValueObject);
            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""myValueObject"":{""id"":2,""name"":""a"",""user"":{""id"":0,""name"":null,""url"":""http://www.example.org/""}}}"), json);
        }

        [Fact]
        public async Task MultiplePropertiesOnSubTypeShouldSerialize()
        {
            var entity = new User
            {
                Department = new Department
                {
                    Id = 1,
                    Name = "Department"
                }
            };

            var fields = ExpressionTree<User>.New(u => u.Department.Id, (User u) => u.Department.Name);
            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""department"":{""id"":1,""name"":""Department""}}"), json);
        }

        [Fact]
        public async Task SubEntityBase_should_serializeIdNameUrl()
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
            var fields = ExpressionTree<User>.New(u => u.Department);
            var json = await SerializeAsync(entity, fields);

            //url is overriden
            Assert.Equal(ExpectedInput(@"{""department"":{""id"":2,""name"":""Foo"",""url"":""http://www.example.org/""}}"), json);
        }

        [Fact]
        public async Task ListSubTypeShouldSerialize()
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

            var fields = ExpressionTree<Department>.New(u => u.Users);
            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""users"":[{""id"":0,""name"":""Peter"",""url"":""http://www.example.org/""},{""id"":1,""name"":""Steven"",""url"":""http://www.example.org/""}]}"), json);
        }

        [Fact]
        public async Task ListSubTypeWithPropertySelectorShouldSerialize()
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

            var fields = ExpressionTree<Department>.New(u => u.Users.Select(g => g.Name));
            var json = await SerializeAsync(entity, fields);

            Assert.Equal(ExpectedInput(@"{""users"":[{""name"":""Peter""},{""name"":""Steven""}]}"), json);
        }
    }
}