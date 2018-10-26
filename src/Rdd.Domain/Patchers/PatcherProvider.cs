using Microsoft.Extensions.DependencyInjection;
using NExtends.Primitives.Types;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using System;
using System.Collections;

namespace Rdd.Domain.Patchers
{
    public class PatcherProvider : IPatcherProvider
    {
        protected IServiceProvider Services { get; set; }
        protected IReflectionHelper ReflectionHelper { get; set; }

        public PatcherProvider(IServiceProvider services, IReflectionHelper reflectionHelper)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            ReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));
        }

        public virtual IPatcher GetPatcher(Type expectedType, IJsonElement json)
        {
            if (typeof(IEntityBase).IsAssignableFrom(expectedType))
            {
                throw new ForbiddenException("It is not permitted to patch a property of type derived from IEntityBase");
            }

            if (expectedType.IsClass)
            {
                var foundPatcher = Services.GetRequiredService(typeof(IPatcher<>).MakeGenericType(new[] { expectedType })) as IPatcher;
                if (foundPatcher != null && foundPatcher.GetType() != typeof(ObjectPatcher<>).MakeGenericType(new[] { expectedType }))
                {
                    return foundPatcher;
                }
            }

            if (json is JsonArray)
            {
                return Services.GetRequiredService<EnumerablePatcher>();
            }

            if (expectedType.IsSubclassOfInterface(typeof(IDictionary)))
            {
                return Services.GetRequiredService<DictionaryPatcher>();
            }

            if (ReflectionHelper.IsPseudoValue(expectedType) || expectedType.GetNullableType().IsValueType)
            {
                return Services.GetRequiredService<ValuePatcher>();
            }

            if (expectedType == typeof(object))
            {
                if (json is JsonValue) { return Services.GetRequiredService<ValuePatcher>(); }
                if (json is JsonObject) { return Services.GetRequiredService<DynamicPatcher>(); }
            }

            return Services.GetRequiredService<ObjectPatcher>();
        }
    }
}