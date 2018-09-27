using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDD.Domain;
using System;

namespace RDD.Web.Models
{
    public class BaseClassJsonConverter<TEntity> : JsonConverter
    {
        private readonly IInheritanceConfiguration<TEntity> _configuration;

        public BaseClassJsonConverter(IInheritanceConfiguration<TEntity> configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override bool CanConvert(Type objectType) => objectType == _configuration.BaseType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            if (!jo.ContainsKey(_configuration.Discriminator))
            {
                throw new ArgumentException($"Correct '{_configuration.Discriminator}' is required as part of the JSON to be able to use this API.");
            }
            var key = jo[_configuration.Discriminator].Value<string>();
            return _configuration.Mappings.ContainsKey(key) ? jo.ToObject(_configuration.Mappings[key], serializer) : null;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
