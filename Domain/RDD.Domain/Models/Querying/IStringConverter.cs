using System;
using System.Collections;
using System.Collections.Generic;

namespace Rdd.Domain.Models.Querying
{
    public interface IStringConverter
    {
        List<T> ConvertValues<T>(IEnumerable<string> values);
        T ConvertTo<T>(string input);

        IList ConvertValues(Type type, IEnumerable<string> values);
        object ConvertTo(Type type, string input);
    }
}