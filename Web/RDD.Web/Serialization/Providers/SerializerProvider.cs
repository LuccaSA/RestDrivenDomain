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
using System.Numerics;

namespace Rdd.Web.Serialization.Providers
{
    public class SerializerProvider : ISerializerProvider
    {
        protected static readonly IReadOnlyCollection<Type> ValueTypes = new HashSet<Type>
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
            typeof(string),
            typeof(byte[]),
            typeof(DBNull),
            //ces cas sont gérés autrement
            typeof(DateTime),
            typeof(DateTime?),
            typeof(Uri),
        };

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
            if (ValueTypes.Contains(type) || type.IsEnum) { return Services.GetService<ValueSerializer>(); }
            if (type.IsEnumerableOrArray()) { return Services.GetService<ArraySerializer>(); }

            return Services.GetService<ObjectSerializer>();
        }
    }
}