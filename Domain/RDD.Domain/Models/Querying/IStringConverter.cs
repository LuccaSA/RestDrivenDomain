using System;
using System.Collections;
using System.Collections.Generic;
using RDD.Domain.Helpers.Expressions;

namespace RDD.Domain.Models.Querying
{
    public interface IStringConverter
    {
        object ConvertTo(Type type, string input);
        T ConvertTo<T>(string input);
        IList ConvertValues(IExpression expression, IEnumerable<string> values);
        List<T> ConvertValues<T>(IExpression expression, IEnumerable<string> values);
    }
}