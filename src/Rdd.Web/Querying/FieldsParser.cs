using Rdd.Domain.Helpers.Expressions;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Rdd.Infra.Storage;
using Rdd.Domain.Exceptions;

namespace Rdd.Web.Querying
{
    public class FieldsParser : IFieldsParser
    {
        protected IExpressionParser ExpressionParser { get; private set; }

        public FieldsParser(IExpressionParser expressionParser)
        {
            ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public virtual IExpressionTree ParseDefaultFields(Type type)
        {
            return ExpressionParser.ParseTree(type, string.Join(",", type.GetProperties().Select(p => p.Name)));
        }
    }

    public class FieldsParser<TEntity> : FieldsParser, IFieldsParser<TEntity>
    {
        private IExpressionTree<TEntity> _defaultFields;
        protected virtual IExpressionTree<TEntity> DefaultFields => _defaultFields ?? (_defaultFields = ExpressionParser.ParseTree<TEntity>(string.Join(",", typeof(TEntity).GetProperties().Select(p => p.Name))));

        protected IPropertyAuthorizer<TEntity> PropertyAuthorizer { get; private set; }

        public FieldsParser(IExpressionParser expressionParser, IPropertyAuthorizer<TEntity> propertyAuthorizer)
            : base(expressionParser)
        {
            PropertyAuthorizer = propertyAuthorizer ?? throw new ArgumentNullException(nameof(propertyAuthorizer));
        }

        public virtual IExpressionTree<TEntity> Parse(HttpRequest request, bool isCollectionCall)
        {
            var result = (request.Query.TryGetValue(Reserved.Fields, out StringValues fieldValue) && !StringValues.IsNullOrEmpty(fieldValue))
                ? ExpressionParser.ParseTree<TEntity>(fieldValue)
                : isCollectionCall 
                    ? new ExpressionTree<TEntity>()
                    : DefaultFields;

            if (result.Any(c => !PropertyAuthorizer.IsVisible(c)))
            {
                throw new BadRequestException($"Fields parsing failed", new ForbiddenException("Selected property is forbidden."));
            }

            return result;
        }
    }
}