using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Web.Serialization.Reflection
{
    public class ReflectionProvider : IReflectionProvider
    {
        private readonly IMemoryCache _cache;

        public ReflectionProvider(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IReadOnlyCollection<PropertyInfo> GetProperties(Type type)
        {
            return _cache.GetOrCreate("reflectionProvider:" + type.AssemblyQualifiedName, c => type.GetProperties());
        }
    }
}