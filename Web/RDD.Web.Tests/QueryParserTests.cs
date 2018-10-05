using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Infra.Helpers;
using RDD.Infra.Web.Models;
using RDD.Web.Querying;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace RDD.Web.Tests
{
    public class QueryParserTests
    {
        [Fact]
        public void CountParseHasOptionImplications()
        {
            var dico = new Dictionary<string, StringValues> { { "fields", "collection.count" } };
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());
            var query = parser.Parse(HttpVerbs.Get, dico, true);

            Assert.True(query.Options.NeedCount);
            Assert.False(query.Options.NeedEnumeration);
        }

        [Fact]
        public void IgnoredAndBadFilters()
        {
            var dico = new Dictionary<string, StringValues> { { "pipo", "nope" }, { "", "oulala" } };
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());
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
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            var query = parser.Parse(httpcontext, true); ;

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
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            var query = parser.Parse(HttpVerbs.Get, dico, true);

            Assert.Single(query.OrderBys);
            Assert.Equal(direction, query.OrderBys[0].Direction);
            Assert.Equal(new ExpressionParser().Parse<User>(output), query.OrderBys[0].Expression);
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
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            Assert.Throws<BadRequestException>(() => parser.Parse(HttpVerbs.Get, dico, true));
        }

        [Theory]
        [InlineData("0,10", 0, 10)]
        [InlineData("-0,10", 0, 10)]
        [InlineData("10,20", 10, 20)]
        [InlineData("20,10", 20, 10)]
        public void CorrectPaging(string input, int offset, int limit)
        {
            var dico = new Dictionary<string, StringValues> { { "paging", input } };
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            var query = parser.Parse(HttpVerbs.Get, dico, true);

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
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            Assert.Throws<BadRequestException>(() => parser.Parse(HttpVerbs.Get, dico, true));
        }

        [Theory]
        [InlineData("iddddd", "zef")]
        [InlineData("id", "zef")]
        [InlineData("id", "between,aaa,zzz")]
        public void InCorrectFilter(string key, string value)
        {
            var dico = new Dictionary<string, StringValues> { { key, value } };
            var parser = new QueryParser<User>(new StringConverter(), new ExpressionParser(), new WebFilterConverter<User>());

            Assert.Throws<BadRequestException>(() => parser.Parse(HttpVerbs.Get, dico, true));
        }
    }
}