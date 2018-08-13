using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Tests.ServerMock;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class FieldsTests
    {
        [Fact]
        public void TwoLevelSelection()
        {
            Obj1 obj1 = new Obj1
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
            ISelection<Obj1> selection = new Selection<Obj1>(new List<Obj1>()
            {
                obj1
            }, 1);
            
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "Fields",new StringValues("Obj2[Id,Name,Obj3[Something,Else],Else]")}
            });

            var httpContextAccessor = new HttpContextAccessor()
            {
                HttpContext = httpContext
            };
            var httpCtx = new HttpContextHelper(httpContextAccessor);

            ApiHelper<Obj1, int> apiHelper = new ApiHelper<Obj1, int>(httpCtx);
            var query = apiHelper.CreateQuery(Domain.Helpers.HttpVerbs.Get, true);

            var serializer = new RDDSerializer(
                new EntitySerializer(
                    new UrlProvider(httpContextAccessor)
                    ), new CurPrincipal());

            var serialized = serializer.Serialize(selection, query);

            var json = JsonConvert.SerializeObject(serialized);

            Assert.Contains("\"A\"", json);
            Assert.Contains("\"B\"", json);
            Assert.Contains("\"else\"", json);
            Assert.Contains("\"2\"", json);
            Assert.DoesNotContain("\"3\"", json);
        }
    }

    public class Obj1 : IEntityBase<int>
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public string Url { get; }
        public Obj2 Obj2 { get; set; }
        public object GetId()
        {
            return Id;
        }

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
