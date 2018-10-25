using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Serialization;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Infra.Helpers;
using Rdd.Web.Querying;
using Rdd.Domain.Tests.Models;
using Rdd.Web.Serialization;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Serializers;
using Rdd.Web.Serialization.UrlProviders;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests.Serialization
{
    public partial class FieldsTests
    {
        protected static readonly DateTime GeneratedAt = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        protected Task<string> SerializeAsync<T>(ISelection<T> selection, IExpressionTree fields)
           where T : class
        {
            return SerializeCorrectedFieldsAsync(new RddJsonResult<T>(selection, fields));
        }

        protected Task<string> SerializeAsync<T>(T data, IExpressionTree fields)
            where T : class
        {
            return SerializeCorrectedFieldsAsync(new RddJsonResult<T>(data, fields));
        }

        protected IServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IInheritanceConfiguration, InheritanceConfiguration>();
            services.AddSingleton<IReflectionHelper, ReflectionHelper>();
            services.AddSingleton<ISerializerProvider, SerializerProvider>();
            services.AddSingleton<NamingStrategy>(new CamelCaseNamingStrategy());
            services.AddSingleton(ArrayPool<char>.Shared);

            services.AddSingleton<ArraySerializer>();
            services.AddSingleton<BaseClassSerializer>();
            services.AddSingleton<CultureInfoSerializer>();
            services.AddSingleton<DictionarySerializer>();
            services.AddSingleton<EntitySerializer>();
            services.AddSingleton<MetadataSerializer>();
            services.AddSingleton<ObjectSerializer>();
            services.AddSingleton<SelectionSerializer>();
            services.AddSingleton<ToStringSerializer>();
            services.AddSingleton<ValueSerializer>();

            services.AddSingleton<IExpressionParser, ExpressionParser>();
            services.AddSingleton(typeof(IWebFilterConverter<>), typeof(WebFilterConverter<>));
            services.AddSingleton<IPagingParser, PagingParser>();
            services.AddSingleton<IFilterParser, FilterParser>();
            services.AddSingleton<IFieldsParser, FieldsParser>();
            services.AddSingleton<IOrderByParser, OrderByParser>();
            services.AddSingleton(typeof(IQueryParser<>), typeof(QueryParser<>));

            var urlProvider = new Mock<IUrlProvider>();
            urlProvider.Setup(u => u.GetEntityApiUri(It.IsAny<IPrimaryKey>())).Returns(new Uri("http://www.example.org/"));
            services.AddSingleton(urlProvider.Object);


            return services.BuildServiceProvider();
        }

        protected async Task<string> SerializeCorrectedFieldsAsync<T>(RddJsonResult<T> result)
           where T : class
        {
            using (var writer = new StringWriter())
            {
                await result.WriteResult(GetServices(), writer, GeneratedAt);

                return writer.ToString();
            }
        }

        protected string ExpectedInput(string expected)
            => @"{""header"":{""generated"":""" + GeneratedAt.ToString("yyyy-MM-ddT00:00:00") + @""",""principal"":null},""data"":" + expected + "}";

        [Fact]
        public async Task SpecialSelectionFields()
        {
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { new Obj1 { Id = 1 }, new Obj1 { Id = 2 } }, 1);

            var fields = new ExpressionParser().ParseTree<Obj1>("collection.count");

            var json = await SerializeAsync(selection, fields);

            Assert.Equal(ExpectedInput(@"{""count"":1}"), json);
        }

        [Fact]
        public async Task EmptyFieldsSelection()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1"
            };
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);

            var fields = new ExpressionParser().ParseTree<Obj1>("");

            var json = await SerializeAsync(selection, fields);

            Assert.Equal(ExpectedInput(@"{""items"":[{""id"":1,""name"":""1"",""url"":""http://www.example.org/""}]}"), json);
        }

        [Fact]
        public async Task EmptyFieldsSingleObject()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1"
            };

            var fields = new ExpressionParser().ParseTree<Obj1>("");

            var json = await SerializeAsync(obj1, fields);
            Assert.Equal(ExpectedInput(@"{""id"":1,""name"":""1"",""url"":""http://www.example.org/""}"), json);
        }

        [Fact]
        public async Task TwoLevelSelection()
        {
            var obj1 = new Obj1
            {
                Id = 1,
                Name = "1",
                Obj2 = new Obj2
                {
                    Id = 2,
                    Name = "2",
                    Something = "something",
                    Else = "else",
                    Obj3 = new Obj3
                    {
                        Id = 3,
                        Name = "3",
                        Something = "A",
                        Else = "B"
                    }
                }
            };
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);

            var fields = new ExpressionParser().ParseTree<Obj1>("Obj2[Id,Name,Obj3[Something,Else],Else]");

            var json = await SerializeAsync(selection, fields);

            Assert.Equal(ExpectedInput(@"{""items"":[{""obj2"":{""id"":2,""name"":""2"",""obj3"":{""something"":""A"",""else"":""B""},""else"":""else""}}]}"), json);
        }

        [Fact]
        public async Task SerializeSubCollections()
        {
            var obj1 = new Obj1
            {
                Obj2s = new List<Obj2>
                {
                    new Obj2
                    {
                        Id = 1,
                        Name = "1"
                    },
                    new Obj2
                    {
                        Id = 2,
                        Name = "2"
                    }
                }
            };

            var selection = new Selection<Obj1>(new List<Obj1> { obj1 }, 1);
            var fields = new ExpressionParser().ParseTree<Obj1>("obj2s[id,name]");
            var json = await SerializeAsync<Obj1>(selection, fields);

            Assert.Equal(ExpectedInput(@"{""items"":[{""obj2s"":[{""id"":1,""name"":""1""},{""id"":2,""name"":""2""}]}]}"), json);
        }
    }

    public class Obj1 : IEntityBase<int>
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public string Url { get; }
        public Obj2 Obj2 { get; set; }

        public List<Obj2> Obj2s { get; set; }

        public object GetId() => Id;

        public void SetId(object id)
        {
            Id = (int)id;
        }
    }

    public class Obj2 : Obj1
    {
        public String Something { get; set; }
        public String Else { get; set; }
        public Obj3 Obj3 { get; set; }
    }

    public class Obj3 : Obj2
    {

    }
}
