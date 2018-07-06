using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain
{
    public interface IPropertySelector
    {
        IEnumerable<IPropertySelector> Children { get; }
        bool HasChild { get; }
        LambdaExpression Lambda { get; set; }

        PropertyInfo GetCurrentProperty();
        void Parse(string field);
    }
}
