using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rdd.Domain.Helpers.Reflection
{
    public interface IReflectionHelper
    {
        IReadOnlyCollection<PropertyInfo> GetProperties(Type type);

        object GetValue(object target, PropertyInfo property);
        void SetValue(object target, PropertyInfo property, object value);
    }
}