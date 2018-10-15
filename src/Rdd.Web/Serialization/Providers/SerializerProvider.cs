using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using Rdd.Domain;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rdd.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected Dictionary<Type, ISerializer> Serializers { get; set; }
        protected IServiceProvider Services { get; set; }
        protected IReflectionHelper IReflectionHelper { get; set; }

        protected IEnumerable<IInheritanceConfiguration> InheritanceConfigurations { get; set; }

        public SerializerProvider(IEnumerable<IInheritanceConfiguration> inheritanceConfigurations, IServiceProvider services, IReflectionHelper reflectionHelper)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            IReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));

            Serializers = new Dictionary<Type, ISerializer>();
            InheritanceConfigurations = inheritanceConfigurations;
        }

        public ISerializer ResolveSerializer(object entity)
        {
            if (entity == null) { return Services.GetService<ValueSerializer>(); }

            var type = entity.GetType();
            if (!Serializers.ContainsKey(type))
            {
                Serializers[type] = GetSerializer(type);
            }

            return Serializers[type];
        }

        public virtual ISerializer GetSerializer(Type type)
        {
            if (typeof(CultureInfo).IsAssignableFrom(type)) { return Services.GetService<CultureInfoSerializer>(); }
            if (typeof(Uri).IsAssignableFrom(type)) { return Services.GetService<ToStringSerializer>(); }
            if (IReflectionHelper.IsPseudoValue(type)) { return Services.GetService<ValueSerializer>(); }

            if (typeof(IDictionary).IsAssignableFrom(type)) { return Services.GetService<DictionarySerializer>(); }
            if (type.IsEnumerableOrArray()) { return Services.GetService<ArraySerializer>(); }

            if (InheritanceConfigurations.Any(c => c.BaseType.IsAssignableFrom(type))) { return Services.GetService<BaseClassSerializer>(); }
            if (typeof(IEntityBase).IsAssignableFrom(type)) { return Services.GetService<EntitySerializer>(); }
            if (typeof(Metadata).IsAssignableFrom(type)) { return Services.GetService<MetadataSerializer>(); }
            if (typeof(ISelection).IsAssignableFrom(type)) { return Services.GetService<SelectionSerializer>(); }

            return Services.GetService<ObjectSerializer>();
        }
    }
}