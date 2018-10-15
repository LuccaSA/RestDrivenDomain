using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Providers;
using System;

namespace Rdd.Web.Serialization.Serializers
{
    public class MetadataSerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }

        public MetadataSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
        {
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            NamingStrategy = namingStrategy ?? throw new ArgumentNullException(nameof(namingStrategy));
        }

        public void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            var content = entity as Metadata;

            writer.WriteStartObject();
            {
                writer.WritePropertyName(GetKey(nameof(Metadata.Header)), true);
                writer.WriteStartObject();
                {
                    writer.WritePropertyName(GetKey(nameof(MetadataHeader.Generated)), true);
                    writer.WriteValue(DateTime.SpecifyKind(content.Header.Generated, DateTimeKind.Unspecified));

                    writer.WritePropertyName(GetKey(nameof(MetadataHeader.Principal)), true);
                    writer.WriteValue(content.Header.Principal);
                }
                writer.WriteEndObject();

                writer.WritePropertyName(GetKey(nameof(Metadata.Data)), true);
                SerializerProvider.ResolveSerializer(content.Data).WriteJson(writer, content.Data, fields);
            }
            writer.WriteEndObject();
        }

        protected string GetKey(string key) => NamingStrategy.GetPropertyName(key, false);
    }
}