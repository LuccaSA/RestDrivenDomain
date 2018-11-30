using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Options = Microsoft.Extensions.Options.Options;

namespace Rdd.Web.Tests
{
    public static class QueryParserHelper
    {
        public static QueryParser<T> GetQueryParser<T>(RddOptions rddOptions = null, IExpressionTree whiteList = null)
            where T : class
        {
            var opt = Options.Create(rddOptions ?? new RddOptions());
            var authorizer = new PropertyAuthorizer<T>(whiteList);
            var parser = new ExpressionParser();
            return new QueryParser<T>(new PagingParser(opt), new FilterParser<T>(new StringConverter(), parser, new WebFilterConverter<T>(), authorizer), new FieldsParser(parser), new OrderByParser<T>(parser, authorizer));
        }
    }
}