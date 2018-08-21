using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Web.Serialization.Reflection
{
    public interface IReflectionProvider
    {
        IReadOnlyCollection<PropertyInfo> GetProperties(Type type);
    }
}