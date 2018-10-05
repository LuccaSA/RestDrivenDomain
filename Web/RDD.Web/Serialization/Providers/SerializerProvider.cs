using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Web.Models;
using RDD.Web.Serialization.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RDD.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected IEnumerable<IInheritanceConfiguration> InheritanceConfigurations { get; set; }
        protected IServiceProvider Services { get; set; }

        public SerializerProvider(IServiceProvider services, IEnumerable<IInheritanceConfiguration> inheritanceConfigurations)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            InheritanceConfigurations = inheritanceConfigurations;
        }

        public ISerializer GetSerializer(object entity)
        {
            if (entity == null) { return ActivatorUtilities.CreateInstance<ValueSerializer>(Services); }

            return GetSerializer(entity.GetType());
        }

        public virtual ISerializer GetSerializer(Type type)
        {
            if (InheritanceConfigurations.Any(c => c.BaseType.IsAssignableFrom(type)))
            {
                return ActivatorUtilities.CreateInstance<BaseClassSerializer>(Services, type);
            }

            if (typeof(Metadata).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<MetadataSerializer>(Services); }
            if (typeof(ISelection).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<SelectionSerializer>(Services); }
            if (typeof(IEntityBase).IsAssignableFrom(type)) { return ActivatorUtilities.CreateInstance<EntitySerializer>(Services, type); }

            if (typeof(CultureInfo).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<CultureInfoSerializer>(Services); }
            if (typeof(Uri).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<ToStringSerializer>(Services); }
            if (typeof(IDictionary).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<DictionarySerializer>(Services); }
            if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTime?).IsAssignableFrom(type)) { return ActivatorUtilities.GetServiceOrCreateInstance<DateTimeSerializer>(Services); }
            if (typeof(string).IsAssignableFrom(type) || type.IsValueType) { return ActivatorUtilities.GetServiceOrCreateInstance<ValueSerializer>(Services); }
            if (type.IsEnumerableOrArray()) { return ActivatorUtilities.GetServiceOrCreateInstance<ArraySerializer>(Services); }

            if (type.IsGenericType && typeof(Func<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return ActivatorUtilities.GetServiceOrCreateInstance(Services, typeof(FuncSerializer<>).MakeGenericType(type.GetGenericArguments())) as ISerializer;
            }

            return ActivatorUtilities.CreateInstance<ObjectSerializer>(Services, type);
        }
    }
}