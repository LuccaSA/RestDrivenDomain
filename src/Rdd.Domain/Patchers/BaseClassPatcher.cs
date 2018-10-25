using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rdd.Domain.Patchers
{
    public class BaseClassPatcher<TEntity> : ObjectPatcher<TEntity>
        where TEntity : class
    {
        private readonly IInheritanceConfiguration<TEntity> _configuration;

        public BaseClassPatcher(IPatcherProvider provider, IReflectionHelper reflectionHelper, IInheritanceConfiguration<TEntity> configuration)
            : base(provider, reflectionHelper)
        {
            _configuration = configuration;
        }

        protected override void PatchKey(Dictionary<string, PropertyInfo> properties, object patchedObject, Type entityType, string key, IJsonElement element)
        {
            if (string.Equals(key, _configuration.Discriminator, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            base.PatchKey(properties, patchedObject, entityType, key, element);
        }
    }
}