using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Json;
using System;
using System.Collections;

namespace Rdd.Domain.Patchers
{
    public class PatcherProvider : IPatcherProvider
    {
        protected IServiceProvider Services { get; set; }

        public PatcherProvider(IServiceProvider services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public virtual IPatcher GetPatcher(Type expectedType, IJsonElement json)
        {
            if (json is JsonArray)
            {
                return Services.GetService<EnumerablePatcher>();
            }

            if (typeof(IEntityBase).IsAssignableFrom(expectedType))
            {
                throw new ForbiddenException("It is not permitted to patch a property of type derived from IEntityBase");
            }

            if (expectedType.IsSubclassOfInterface(typeof(IDictionary)))
            {
                return Services.GetService<DictionaryPatcher>();
            }

            if (expectedType == typeof(string) || expectedType.IsValueType || expectedType.GetNullableType().IsValueType)
            {
                return Services.GetService<ValuePatcher>();
            }

            if (expectedType == typeof(object))
            {
                if (json is JsonValue) { return Services.GetService<ValuePatcher>(); }
                if (json is JsonObject) { return Services.GetService<DynamicPatcher>(); }
            }

            return Services.GetService<ObjectPatcher>();
        }
    }
}