using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain
{
    public interface IPropertySelector
    {
        string Name { get; }
        IPropertySelector Child { get; }
        bool HasChild { get; }
        LambdaExpression Lambda { get; set; }

        PropertyInfo GetCurrentProperty();
        void Parse(string field);
    }
}
