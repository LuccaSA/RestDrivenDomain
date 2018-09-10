using RDD.Domain.Helpers.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser
    {
        public IExpressionTree<TClass> ParseFields<TClass>(Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields<TClass>(parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties<TClass>();
            }

            return new ExpressionTree<TClass>();
        }

        private IExpressionTree<TClass> ParseAllProperties<TClass>()
        {
            var fields = string.Join(",", typeof(TClass).GetProperties().Select(p => p.Name));
            return ParseFields<TClass>(fields);
        }

        private IExpressionTree<TClass> ParseFields<TClass>(string fields)
        {
            return new ExpressionParser().ParseTree<TClass>(fields);
        }
    }
}