using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace Rdd.Domain.Helpers.Reflection
{
    public class ReflectionHelper : IReflectionHelper
    {
        protected static readonly HashSet<Type> ValueTypes = new HashSet<Type>
        {
            typeof(char),
            typeof(char?),
            typeof(bool),
            typeof(bool?),
            typeof(sbyte),
            typeof(sbyte?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(byte),
            typeof(byte?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(decimal),
            typeof(decimal?),
            typeof(Guid),
            typeof(Guid?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(BigInteger),
            typeof(BigInteger?),
            typeof(string)
        };

        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesByType;
        private readonly ConcurrentDictionary<PropertyInfo, ExpressionValueProvider> _providersByProperty;

        public ReflectionHelper()
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

        public virtual bool IsPseudoValue(Type type)
            => ValueTypes.Contains(type) || type.IsEnum;
    }
}