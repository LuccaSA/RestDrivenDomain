using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using Rdd.Domain;
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
                return Services.GetService<BaseClassSerializer>();
            }

            if (typeof(Metadata).IsAssignableFrom(type)) { return Services.GetService<MetadataSerializer>(); }
            if (typeof(ISelection).IsAssignableFrom(type)) { return Services.GetService<SelectionSerializer>(); }
            if (typeof(IEntityBase).IsAssignableFrom(type)) { return Services.GetService<EntitySerializer>(); }

            if (typeof(CultureInfo).IsAssignableFrom(type)) { return Services.GetService<CultureInfoSerializer>(); }
            if (typeof(Uri).IsAssignableFrom(type)) { return Services.GetService<ToStringSerializer>(); }
            if (typeof(IDictionary).IsAssignableFrom(type)) { return Services.GetService<DictionarySerializer>(); }
            if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTime?).IsAssignableFrom(type)) { return Services.GetService<DateTimeSerializer>(); }
            if (typeof(string).IsAssignableFrom(type) || type.IsValueType) { return Services.GetService<ValueSerializer>(); }
            if (type.IsEnumerableOrArray()) { return Services.GetService<ArraySerializer>(); }

            return Services.GetService<ObjectSerializer>();
        }
    }
}