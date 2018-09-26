using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Controllers;
using RDD.Web.Querying;
using RDD.Web.Tests.Models;
using Xunit;

namespace RDD.Web.Tests.Serialization
{
    public class FieldsTests
    {
        public FieldsTests()
        {
            _httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            _httpContextAccessor.HttpContext.Request.Scheme = "https";
            _httpContextAccessor.HttpContext.Request.Host = new HostString("mon.domain.com");
        }

        private readonly HttpContextAccessor _httpContextAccessor;

        private PropertyTreeNode FieldsFromQuery(string fields)
        {
            var dic = new Dictionary<string, StringValues>();
            if (fields != null)
            {
                dic.Add(QueryTokens.Fields, fields);
            }
            _httpContextAccessor.HttpContext.Request.Query = new QueryCollection(dic);
            return new FieldsParser(_httpContextAccessor).ParseFields();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EmptyFieldsSelection(string field)
        {
            var paths = FieldsFromQuery(field).AsExpandedPaths();
            Assert.Empty(paths);

            var pathsType = FieldsFromQuery(field).AsExpandedPaths<Obj1>();
            Assert.Empty(pathsType);
        }

        [Fact]
        public void TwoLevelSelection()
        {
            void AssertPaths(IEnumerable<string> extracted)
            {
                Assert.Contains("Obj2.Obj3.Something", extracted);
                Assert.Contains("Obj2.Obj3.Else", extracted);
                Assert.Contains("Obj2.Else", extracted);
                Assert.Contains("Obj2.Name", extracted);
                Assert.DoesNotContain("Obj2.Obj3.Name", extracted);
            }

            var pathsUntyped = FieldsFromQuery("Obj2[Id,Name,Obj3[Something,Else],Else]").AsExpandedPaths();
            AssertPaths(pathsUntyped);

            var pathsTyped = FieldsFromQuery("Obj2[Id,Name,Obj3[Something,Else],Else]").AsExpandedPaths<Obj1>();
            AssertPaths(pathsTyped);

            var pathsTypedExclusion = FieldsFromQuery("Obj2[Id,Name,Obj3[DoesNotExists,Else],Else]").AsExpandedPaths<Obj1>();
            Assert.DoesNotContain("Obj2.Obj3.DoesNotExists", pathsTypedExclusion);
        }
    }

    public class Obj1 : IEntityBase<int>
    {
        public Obj2 Obj2 { get; set; }

        public List<Obj2> Obj2s { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; }

        public object GetId() => Id;

        public void SetId(object id)
        {
            Id = (int)id;
        }
    }

    public class Obj2 : Obj1
    {
        public string Something { get; set; }
        public string Else { get; set; }
        public Obj3 Obj3 { get; set; }
    }

    public class Obj3 : Obj2
    {}
}
