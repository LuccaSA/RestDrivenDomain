using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Web.Serialization.Reflection
{
    public class ReflectionProvider : IReflectionProvider
    {
        Dictionary<Type, PropertyInfo[]> Cache { get; set; }

        public ReflectionProvider()
        {
            Cache = new Dictionary<Type, PropertyInfo[]>();
        }

        public IReadOnlyCollection<PropertyInfo> GetProperties(Type type)
        {
            if (!Cache.ContainsKey(type))
            {
                Cache[type] = type.GetProperties();
            }
            return Cache[type];
        }
    }
}