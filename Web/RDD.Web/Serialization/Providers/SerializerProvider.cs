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
        protected IServiceProvider Services { get; set; }
        protected IReflectionHelper IReflectionHelper { get; set; }

        protected IEnumerable<IInheritanceConfiguration> InheritanceConfigurations { get; set; }

        public SerializerProvider(IEnumerable<IInheritanceConfiguration> inheritanceConfigurations, IServiceProvider services, IReflectionHelper reflectionHelper)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            IReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));

            InheritanceConfigurations = inheritanceConfigurations;
        }

        public ISerializer GetSerializer(object entity)
        {
            if (entity == null) { return Services.GetService<ValueSerializer>(); }

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
            if (IReflectionHelper.IsPseudoValue(type)) { return Services.GetService<ValueSerializer>(); }
            if (type.IsEnumerableOrArray()) { return Services.GetService<ArraySerializer>(); }

            return Services.GetService<ObjectSerializer>();
        }
    }
}