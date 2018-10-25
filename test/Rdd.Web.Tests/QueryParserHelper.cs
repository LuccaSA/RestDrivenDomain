using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Helpers;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using System;
using Options = Microsoft.Extensions.Options.Options;

namespace Rdd.Web.Tests
{
    public static class QueryParserHelper
    {
        public static QueryParser<TEntity, TKey> GetQueryParser<TEntity, TKey>(RddOptions rddOptions = null)
            where TEntity : class, IPrimaryKey<TKey>
            where TKey : IEquatable<TKey>
        {
            var opt = Options.Create(rddOptions ?? new RddOptions());
            return new QueryParser<TEntity, TKey>(new WebFilterConverter<TEntity>(), new PagingParser(opt), new FilterParser(new StringConverter(), new ExpressionParser()), new FieldsParser(new ExpressionParser()), new OrderByParser(new ExpressionParser()));
        }
    }
}