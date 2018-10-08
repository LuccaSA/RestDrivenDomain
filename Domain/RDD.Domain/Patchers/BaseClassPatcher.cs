using Rdd.Domain.Json;
using System.Reflection;

namespace Rdd.Domain.Patchers
{
    public class BaseClassPatcher<TEntity> : ObjectPatcher<TEntity>
        where TEntity : class
    {
        private readonly IInheritanceConfiguration<TEntity> _configuration;

        public BaseClassPatcher(IPatcherProvider provider, IInheritanceConfiguration<TEntity> configuration)
            : base(provider)
        {
            _configuration = configuration;
        }

        protected override void PatchProperty(object patchedObject, PropertyInfo property, IJsonElement element)
        {
            if (property.Name.ToUpper() == _configuration.Discriminator.ToUpper())
            {
                return;
            }
            base.PatchProperty(patchedObject, property, element);
        }
    }
}
