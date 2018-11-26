using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using Rdd.Domain;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Serializers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rdd.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected ConcurrentDictionary<Type, ISerializer> Serializers { get; }
        protected IServiceProvider Services { get; set; }
        protected IReflectionHelper ReflectionHelper { get; set; }

        protected IEnumerable<IInheritanceConfiguration> InheritanceConfigurations { get; set; }

        public SerializerProvider(IEnumerable<IInheritanceConfiguration> inheritanceConfigurations, IServiceProvider services, IReflectionHelper reflectionHelper)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            ReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));

            Serializers = new ConcurrentDictionary<Type, ISerializer>();
            InheritanceConfigurations = inheritanceConfigurations;
        }

        public ISerializer ResolveSerializer(object entity)
        {
            if (entity == null) { return Services.GetRequiredService<ValueSerializer>(); }

            var type = entity.GetType();

            if(Serializers.TryGetValue(type, out var serializer))
            {
                return serializer;
            }

            var newSerializerInstance =  GetSerializer(type);

            Serializers.TryAdd(type, newSerializerInstance);
            
            return newSerializerInstance;
        }

        public virtual ISerializer GetSerializer(Type type)
        {
            if (typeof(CultureInfo).IsAssignableFrom(type)) { return Services.GetRequiredService<CultureInfoSerializer>(); }
            if (typeof(Uri).IsAssignableFrom(type)) { return Services.GetRequiredService<ToStringSerializer>(); }
            if (ReflectionHelper.IsPseudoValue(type)) { return Services.GetRequiredService<ValueSerializer>(); }

            if (typeof(IDictionary).IsAssignableFrom(type)) { return Services.GetRequiredService<DictionarySerializer>(); }
            if (type.IsEnumerableOrArray()) { return Services.GetRequiredService<ArraySerializer>(); }

            if (InheritanceConfigurations.Any(c => c.BaseType.IsAssignableFrom(type))) { return Services.GetRequiredService<BaseClassSerializer>(); }
            if (typeof(IEntityBase).IsAssignableFrom(type)) { return Services.GetRequiredService<EntitySerializer>(); }
            if (typeof(Metadata).IsAssignableFrom(type)) { return Services.GetRequiredService<MetadataSerializer>(); }
            if (typeof(ISelection).IsAssignableFrom(type)) { return Services.GetRequiredService<SelectionSerializer>(); }

            return Services.GetRequiredService<ObjectSerializer>();
        }
    }
}