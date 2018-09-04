using RDD.Domain.Helpers.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser
    {
        public IExpressionSelectorTree ParseFields(Type classType, Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields(classType, parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties(classType);
            }

            return new ExpressionSelectorTree();
        }

        private IExpressionSelectorTree ParseAllProperties(Type classType)
        {
            var fields = string.Join(",", classType.GetProperties().Select(p => p.Name));
            return ParseFields(classType, fields);
        }

        private IExpressionSelectorTree ParseFields(Type classType, string fields)
        {
            return new ExpressionSelectorParser().ParseTree(classType, fields);
        }
    }
}