using Rdd.Domain.Helpers.Expressions;
using System;
using System.Linq;

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
                return Parse<TEntity>(string.Join(",", typeof(TEntity).GetProperties().Select(p => p.Name)));
            }
            else
            {
                return new ExpressionTree<TEntity>();
            }
        }

        public virtual IExpressionTree<TEntity> Parse<TEntity>(string fields) => ExpressionParser.ParseTree<TEntity>(fields);
    }
}