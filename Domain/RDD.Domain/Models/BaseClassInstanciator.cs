using System;

namespace RDD.Domain.Models
{
    public class BaseClassInstanciator<TEntity> : IInstanciator<TEntity>
        where TEntity : class
    {
        private readonly IInheritanceConfiguration<TEntity> _configuration;

        public BaseClassInstanciator(IInheritanceConfiguration<TEntity> configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public TEntity InstanciateNew(ICandidate<TEntity> candidate)
        {
            if (candidate.JsonValue.HasJsonValue(_configuration.Discriminator))
            {
                var value = candidate.JsonValue.GetJsonValue(_configuration.Discriminator);
                if (_configuration.Mappings.ContainsKey(value))
                {
                    return Activator.CreateInstance(_configuration.Mappings[value]) as TEntity;
                }
            }

            throw new ArgumentException($"Correct '{_configuration.Discriminator}' is required as part of the JSON to be able to use this API.");
        }
    }
}