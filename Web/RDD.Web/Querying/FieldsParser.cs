using RDD.Domain.Helpers.Expressions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace RDD.Web.Querying
{
    public class FieldsParser : IFieldsParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FieldsParser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IExpressionTree<T> ParseFields<T>() where T : class
        {
            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue(QueryTokens.Fields, out StringValues fieldsValues))
            {
                return ParseFields<T>(fieldsValues);
            }
            return ParseAllProperties<T>();
        }

        private IExpressionTree<T> ParseAllProperties<T>() where T : class
        {
            var fields = string.Join(",", typeof(T).GetProperties().Select(p => p.Name));
            return ParseFields<T>(fields);
        }

        private IExpressionTree<T> ParseFields<T>(string fields) where T : class
        {
            return new ExpressionParser().ParseTree<T>(fields);
        }
    }

    public interface IFieldsParser
    {
        IExpressionTree<T> ParseFields<T>() where T : class;
    }
}