using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rdd.Web.Helpers;
using Xunit;

namespace Rdd.Web.Tests
{
    public class QueryParserTests
    {
 
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var request = HttpVerbs.Get.NewRequest(("fields", "collection.count")); 
            var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);

            Assert.True(query.Options.NeedCount);
            Assert.False(query.Options.NeedEnumeration);
        }

        [Fact]
        public void IgnoredAndBadFilters()
        {
            var request = HttpVerbs.Get.NewRequest(   ("", "oulala") );

            var options = new RddOptions();
            var parser = QueryParserHelper.GetQueryParser<User>(options);

            var query = parser.Parse(request, true);
        }

        [Theory]
        [InlineData(HttpVerbs.Get)]
        [InlineData(HttpVerbs.Post)]
        [InlineData(HttpVerbs.Put)]
        [InlineData(HttpVerbs.Delete)]
        [InlineData(HttpVerbs.None)]
        public void CorrectVerb(HttpVerbs input)
        {
            var query = QueryParserHelper.GetQueryParser<User>().Parse(input.NewRequest(), true);

            Assert.Equal(query.Verb, input);
        }

        [Theory]
        [InlineData("id,asc", SortDirection.Ascending, "id")]
        [InlineData("id,desc", SortDirection.Descending, "id")]
        [InlineData("name,asc", SortDirection.Ascending, "name")]
        [InlineData("Salary,asc", SortDirection.Ascending, "Salary")]
        [InlineData("Department.id,asc", SortDirection.Ascending, "Department.id")]
        [InlineData("PictureId,asc", SortDirection.Ascending, "PictureId")]
        [InlineData("BirthDay,asc", SortDirection.Ascending, "BirthDay")]
        [InlineData("ContractStart,asc", SortDirection.Ascending, "ContractStart")]
        public void CorrectOrderBys(string input, SortDirection direction, string output)
        {
            var request = HttpVerbs.Get.NewRequest(("orderby", input));
            var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);

            Assert.Single(query.OrderBys);
            Assert.Equal(direction, query.OrderBys[0].Direction);
            Assert.Equal
            (
                new PropertyExpression(new ExpressionParser().Parse<User>(output).ToLambdaExpression()).Property,
                new PropertyExpression(query.OrderBys[0].LambdaExpression).Property
            );
        }

        [Theory]
        [InlineData("aaaa")]
        [InlineData("id")]
        [InlineData("id,")]
        [InlineData("id,,")]
        [InlineData("id,ascending")]
        [InlineData("id,zerferferf")]
        [InlineData("fff,asc")]
        [InlineData("MyValueObject,asc")]
        [InlineData("TwitterUri,asc")]
        [InlineData("Department,asc")]
        [InlineData("Department.Users,asc")]
        public void IncorrectOrderBys(string input)
        {
            var request = HttpVerbs.Get.NewRequest(("orderby", input));
            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(request, true));
        }

        [Theory]
        [InlineData(HttpVerbs.Get, 0, 10)]
        [InlineData(HttpVerbs.Post, 0, 0)]
        [InlineData(HttpVerbs.Put, 0, 0)]
        [InlineData(HttpVerbs.Delete, 0, 0)]
        public void DefaultPaging(HttpVerbs verb, int offset, int limit)
        {
            var request = HttpRequestHelper.NewRequest(verb);
            var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);

            Assert.Equal(offset, query.Page.Offset);
            Assert.Equal(limit, query.Page.Limit);
        }

        [Theory]
        [InlineData("1", 0, 10)]
        [InlineData("0,10", 0, 10)]
        [InlineData("-0,10", 0, 10)]
        [InlineData("10,20", 10, 20)]
        [InlineData("20,10", 20, 10)]
        public void CorrectPaging(string input, int offset, int limit)
        {
            var request = HttpVerbs.Get.NewRequest(("paging", input)); 

            var query = QueryParserHelper.GetQueryParser<User>().Parse(request, true);

            Assert.Equal(offset, query.Page.Offset);
            Assert.Equal(limit, query.Page.Limit);
        }

        [Theory]
        [InlineData("aaaa")]
        [InlineData("0,")]
        [InlineData(",0")]
        [InlineData("-1,10")]
        [InlineData("0,99999999")]
        public void IncorrectPaging(string input)
        {
            var request = HttpVerbs.Get.NewRequest(("paging", input));

            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(request, true));
        }

        [Theory]
        [InlineData("iddddd", "zef")]
        [InlineData("id", "zef")]
        [InlineData("id", "between,aaa,zzz")]
        public void InCorrectFilter(string key, string value)
        {
            var request = HttpVerbs.Get.NewRequest((key, value)); 

            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(request, true));
        }
    }
}