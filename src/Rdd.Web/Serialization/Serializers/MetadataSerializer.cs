using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Providers;
using System;
using System.Threading.Tasks;

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

        public async Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            var content = entity as Metadata;

            await writer.WriteStartObjectAsync();
            {
                await writer.WritePropertyNameAsync(GetKey(nameof(Metadata.Header)), true);
                await writer.WriteStartObjectAsync();
                {
                    await writer.WritePropertyNameAsync(GetKey(nameof(MetadataHeader.Generated)), true);
                    await writer.WriteValueAsync(DateTime.SpecifyKind(content.Header.Generated, DateTimeKind.Unspecified));

                    await writer.WritePropertyNameAsync(GetKey(nameof(MetadataHeader.Principal)), true);
                    await writer.WriteValueAsync(content.Header.Principal);
                }
                await writer.WriteEndObjectAsync();

                await writer.WritePropertyNameAsync(GetKey(nameof(Metadata.Data)), true);
                await SerializerProvider.ResolveSerializer(content.Data).WriteJsonAsync(writer, content.Data, fields);
            }
            await writer.WriteEndObjectAsync();
        }

        protected string GetKey(string key) => NamingStrategy.GetPropertyName(key, false);
    }
}