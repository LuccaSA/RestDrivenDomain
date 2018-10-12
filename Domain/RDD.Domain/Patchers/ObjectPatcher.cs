using Rdd.Domain.Exceptions;
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

        public ObjectPatcher(IPatcherProvider provider)
        {
            Provider = provider;
        }

        object IPatcher.InitialValue(PropertyInfo property, object patchedObject)
        {
            return property.GetValue(patchedObject);
        }

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
            var properties = entityType.GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

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
            var propertySetter = property.GetSetMethod();
            if (propertySetter == null)
                throw new ForbiddenException($"Property {property.Name} of type {property.DeclaringType.Name} is not writable");

            var patcher = Provider.GetPatcher(property.PropertyType, element);
            var initialValue = patcher.InitialValue(property, patchedObject);
            var value = patcher.PatchValue(initialValue, property.PropertyType, element);

            PatchProperty(propertySetter, patchedObject, value, property);
        }

        protected virtual void PatchProperty(MethodInfo propertySetter, object patchedObject, object value, PropertyInfo property)
        {
            propertySetter.Invoke(patchedObject, new[] { value });
        }
    }

    public class ObjectPatcher<T> : ObjectPatcher, IPatcher<T>
        where T : class
    {
        public ObjectPatcher(IPatcherProvider provider) : base(provider)
        {
        }

        public T Patch(T patchedObject, JsonObject json)
            => PatchValue(patchedObject, patchedObject.GetType(), json) as T;
    }
}