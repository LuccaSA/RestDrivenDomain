using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Rdd.Domain.Helpers.Reflection
{
    public class ReflectionProvider : IReflectionProvider
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesByType;
        private readonly ConcurrentDictionary<PropertyInfo, ExpressionValueProvider> _providersByProperty;

        public ReflectionProvider()
        {
            _propertiesByType = new ConcurrentDictionary<Type, PropertyInfo[]>();
            _providersByProperty = new ConcurrentDictionary<PropertyInfo, ExpressionValueProvider>();
        }

        public virtual IReadOnlyCollection<PropertyInfo> GetProperties(Type type)
            => _propertiesByType.GetOrAdd(type, t => t.GetProperties());

        public virtual object GetValue(object target, PropertyInfo property)
            => _providersByProperty.GetOrAdd(property, p => new ExpressionValueProvider(p)).GetValue(target);

        public virtual void SetValue(object target, PropertyInfo property, object value)
            => _providersByProperty.GetOrAdd(property, p => new ExpressionValueProvider(p)).SetValue(target, value);
    }
}