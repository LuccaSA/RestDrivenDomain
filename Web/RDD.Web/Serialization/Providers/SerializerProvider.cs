using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RDD.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected IReflectionProvider ReflectionProvider { get; set; }
        protected IUrlProvider UrlProvider { get; set; }
        protected IEnumerable<IInheritanceConfiguration> InheritanceConfigurations { get; set; }

        public SerializerProvider(IReflectionProvider reflectionProvider, IUrlProvider urlProvider, IEnumerable<IInheritanceConfiguration> inheritanceConfigurations)
        {
            ReflectionProvider = reflectionProvider ?? throw new ArgumentNullException(nameof(reflectionProvider));
            UrlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));

            InheritanceConfigurations = inheritanceConfigurations;
        }

        public ISerializer GetSerializer(object entity)
        {
            if (entity == null) { return new ValueSerializer(this); }

            return GetSerializer(entity.GetType());
        }

        public virtual ISerializer GetSerializer(Type type)
        {
            if (typeof(CultureInfo).IsAssignableFrom(type)) { return new CultureInfoSerializer(this, ReflectionProvider); }
            if (typeof(Uri).IsAssignableFrom(type)) { return new ToStringSerializer(this); }

            if (typeof(ISelection).IsAssignableFrom(type)) { return new SelectionSerializer(this, ReflectionProvider); }

            var inheritanceConfig = InheritanceConfigurations.FirstOrDefault(c => c.BaseType.IsAssignableFrom(type));
            if (inheritanceConfig != null)
            {
                return new BaseClassSerializer(this, ReflectionProvider, UrlProvider, type);
            }

            if (typeof(IEntityBase).IsAssignableFrom(type)) { return new EntitySerializer(this, ReflectionProvider, UrlProvider, type); }

            if (typeof(IDictionary).IsAssignableFrom(type)) { return new DictionarySerializer(this, ReflectionProvider); }
            if (type.IsEnumerableOrArray()) { return new ArraySerializer(this); }
            if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTime?).IsAssignableFrom(type)) { return new DateTimeSerializer(this); }
            if (typeof(string).IsAssignableFrom(type) || type.IsValueType) { return new ValueSerializer(this); }

            if (type.IsGenericType && typeof(Func<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return Activator.CreateInstance(typeof(FuncSerializer<>).MakeGenericType(type.GetGenericArguments()), this) as ISerializer;
            }

            return new ObjectSerializer(this, ReflectionProvider, type);
        }
    }
}