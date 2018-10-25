using Rdd.Domain.Helpers.Expressions;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Rdd.Web.Querying
{
    public class FieldsParser : IFieldsParser
    {
        protected IExpressionParser ExpressionParser { get; private set; }

        public FieldsParser(IExpressionParser expressionParser)
        {
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public virtual IExpressionTree<TEntity> GetDeFaultFields<TEntity>(bool isCollectionCall)
        {
            if (!isCollectionCall)
            { 
                return ExpressionParser.ParseTree<TEntity>(string.Join(",", typeof(TEntity).GetProperties().Select(p => p.Name)));
            }
            return new ExpressionTree<TEntity>();
        }

        public virtual IExpressionTree ParseDefaultFields(Type type)
        {
            return ExpressionParser.ParseTree(type, string.Join(",", type.GetProperties().Select(p => p.Name)));
        }

        public virtual IExpressionTree<TEntity> Parse<TEntity>(HttpRequest request, bool isCollectionCall)
        {
            if (request.Query.TryGetValue(Reserved.Fields, out StringValues fieldValue) && !StringValues.IsNullOrEmpty(fieldValue))
            {
                return ExpressionParser.ParseTree<TEntity>(fieldValue);
            }
            return GetDeFaultFields<TEntity>(isCollectionCall);
        }
    }
}