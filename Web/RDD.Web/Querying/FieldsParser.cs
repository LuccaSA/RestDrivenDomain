using RDD.Domain.Helpers.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser<T>
    {
        public IExpressionTree<T> ParseFields(Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields(parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties();
            }

            return new ExpressionTree<T>();
        }

        private IExpressionTree<T> ParseAllProperties()
        {
            var fields = string.Join(",", typeof(T).GetProperties().Select(p => p.Name));
            return ParseFields(fields);
        }

        private IExpressionTree<T> ParseFields(string fields)
        {
            return new ExpressionParser().ParseTree<T>(fields);
        }
    }
}