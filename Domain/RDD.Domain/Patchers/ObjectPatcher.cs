using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rdd.Domain.Patchers
{
    public class ObjectPatcher : IPatcher
    {
        protected IPatcherProvider Provider { get; set; }
        protected IReflectionProvider ReflectionProvider { get; set; }

        public ObjectPatcher(IPatcherProvider provider, IReflectionProvider reflectionProvider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ReflectionProvider = reflectionProvider ?? throw new ArgumentNullException(nameof(reflectionProvider));
        }

        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
            => ReflectionProvider.GetValue(patchedObject, property);

        object IPatcher.PatchValue(object patchedObject, Type expectedType, IJsonElement json)
        {
            return PatchValue(patchedObject, expectedType, json as JsonObject);
        }

        public virtual object PatchValue(object patchedObject, Type expectedType, JsonObject json)
        {
            if (json == null)
                return null;

            if (patchedObject == null)
            {
                patchedObject = Activator.CreateInstance(expectedType);
            }

            var entityType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
            var properties = ReflectionProvider.GetProperties(entityType).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in GetKvps(expectedType, json.Content))
            {
                PatchKey(properties, patchedObject, entityType, kvp.Key, kvp.Value);
            }

            return patchedObject;
        }

        /// <summary>
        /// Allows for reordering of the keys
        /// </summary>
        /// <param name="expectedType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual IEnumerable<KeyValuePair<string, IJsonElement>> GetKvps(Type expectedType, Dictionary<string, IJsonElement> content) => content;

        protected virtual void PatchKey(Dictionary<string, PropertyInfo> properties, object patchedObject, Type entityType, string key, IJsonElement element)
        {
            if (!properties.ContainsKey(key))
                throw new BadRequestException($"Property {key} does not exist on type {entityType.Name}");

            PatchProperty(patchedObject, properties[key], element);
        }

        protected virtual void PatchProperty(object patchedObject, PropertyInfo property, IJsonElement element)
        {
            var patcher = Provider.GetPatcher(property.PropertyType, element);
            var initialValue = patcher.InitialValue(property, patchedObject);
            var value = patcher.PatchValue(initialValue, property.PropertyType, element);

            PatchProperty(patchedObject, value, property);
        }

        protected virtual void PatchProperty(object patchedObject, object value, PropertyInfo property)
            => ReflectionProvider.SetValue(patchedObject, property, value);
    }

    public class ObjectPatcher<T> : ObjectPatcher, IPatcher<T>
        where T : class
    {
        public ObjectPatcher(IPatcherProvider provider, IReflectionProvider reflectionProvider)
            : base(provider, reflectionProvider)
        {
        }

        public T Patch(T patchedObject, JsonObject json)
            => PatchValue(patchedObject, patchedObject.GetType(), json) as T;
    }
}