using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace Rdd.Web.Tests
{
    public class QueryParserTests
    {
 
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var dico = new Dictionary<string, StringValues> { { "fields", "collection.count" } };
            var query = QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true);

            Assert.True(query.Options.NeedCount);
            Assert.False(query.Options.NeedEnumeration);
        }

        [Fact]
        public void IgnoredAndBadFilters()
        {
            var dico = new Dictionary<string, StringValues> { { "pipo", "nope" }, { "", "oulala" } };
            var parser = QueryParserHelper.GetQueryParser<User>();
            parser.IgnoreFilters("pipo");

            var query = parser.Parse(HttpVerbs.Get, dico, true);

            //no bug
        }

        [Theory]
        [InlineData(HttpVerbs.Get)]
        [InlineData(HttpVerbs.Post)]
        [InlineData(HttpVerbs.Put)]
        [InlineData(HttpVerbs.Delete)]
        [InlineData(HttpVerbs.None)]
        public void CorrectVerb(HttpVerbs input)
        {
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Method = input.ToString();

            var query = QueryParserHelper.GetQueryParser<User>().Parse(httpcontext, true); ;

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
            var dico = new Dictionary<string, StringValues> { { "orderby", input } };

            var query = QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true);

            Assert.Single(query.OrderBys);
            Assert.Equal(direction, query.OrderBys[0].Direction);
            Assert.Equal
            (
                new PropertyExpression(new ExpressionParser().Parse<User>(output).ToLambdaExpression()).Property,
                new PropertyExpression(query.OrderBys[0].LambdaExpression).Property
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
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
            var dico = new Dictionary<string, StringValues> { { "orderby", input } };

            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true));
        }

        [Theory]
        [InlineData("0,10", 0, 10)]
        [InlineData("-0,10", 0, 10)]
        [InlineData("10,20", 10, 20)]
        [InlineData("20,10", 20, 10)]
        public void CorrectPaging(string input, int offset, int limit)
        {
            var dico = new Dictionary<string, StringValues> { { "paging", input } };

            var query = QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true);

            Assert.Equal(offset, query.Page.Offset);
            Assert.Equal(limit, query.Page.Limit);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("aaaa")]
        [InlineData("0,")]
        [InlineData(",0")]
        [InlineData("-1,10")]
        [InlineData("0,99999999")]
        public void IncorrectPaging(string input)
        {
            var dico = new Dictionary<string, StringValues> { { "paging", input } };

            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true));
        }

        [Theory]
        [InlineData("iddddd", "zef")]
        [InlineData("id", "zef")]
        [InlineData("id", "between,aaa,zzz")]
        public void InCorrectFilter(string key, string value)
        {
            var dico = new Dictionary<string, StringValues> { { key, value } };

            Assert.Throws<BadRequestException>(() => QueryParserHelper.GetQueryParser<User>().Parse(HttpVerbs.Get, dico, true));
        }
    }
}