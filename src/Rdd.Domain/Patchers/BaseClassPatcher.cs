using System;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
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

        protected override void PatchProperty(object patchedObject, PropertyInfo property, IJsonElement element)
        {
            if (String.Equals(property.Name, _configuration.Discriminator, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            base.PatchProperty(patchedObject, property, element);
        }
    }
}
