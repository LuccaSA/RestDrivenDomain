using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Collections;
using System.Globalization;

namespace RDD.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected IReflectionProvider _reflectionProvider;
        protected IUrlProvider _urlProvider;

        public SerializerProvider(IReflectionProvider reflectionProvider, IUrlProvider urlProvider)
        {
            _reflectionProvider = reflectionProvider ?? throw new ArgumentNullException(nameof(reflectionProvider));
            _urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
        }

        public ISerializer GetSerializer(object entity)
        {
            if (entity == null) { return new ValueSerializer(this); }

            return GetSerializer(entity.GetType());
        }

        public virtual ISerializer GetSerializer(Type type)
        {
            if (typeof(CultureInfo).IsAssignableFrom(type)) { return new CultureInfoSerializer(this, _reflectionProvider); }
            if (typeof(Uri).IsAssignableFrom(type)) { return new ToStringSerializer(this); }

            if (typeof(ISelection).IsAssignableFrom(type)) { return new SelectionSerializer(this, _reflectionProvider); }
            if (typeof(IEntityBase).IsAssignableFrom(type)) { return new EntitySerializer(this, _reflectionProvider, _urlProvider, type); }

            if (typeof(IDictionary).IsAssignableFrom(type)) { return new DictionarySerializer(this, _reflectionProvider); }
            if (type.IsEnumerableOrArray()) { return new ArraySerializer(this); }
            if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTime?).IsAssignableFrom(type)) { return new DateTimeSerializer(this); }
            if (typeof(string).IsAssignableFrom(type) || type.IsValueType) { return new ValueSerializer(this); }

            if (type.IsGenericType && typeof(Func<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return Activator.CreateInstance(typeof(FuncSerializer<>).MakeGenericType(type.GetGenericArguments()), this) as ISerializer;
            }

            return new ObjectSerializer(this, _reflectionProvider, type);
        }
    }
}