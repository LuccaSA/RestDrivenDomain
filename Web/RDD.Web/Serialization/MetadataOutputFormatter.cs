using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Models;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Provides RDD default Output formatter to control serialized info and metadatas
    /// </summary>
    public class MetadataOutputFormatter : OutputFormatter
    {
        public MetadataOutputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            Query query = context.HttpContext.GetContextualQuery();

            var entitySerializer = context.HttpContext.GetService<IEntitySerializer>();
            var execution = context.HttpContext.GetService<IExecutionContext>();

            Metadata metadata;
            if (typeof(IEnumerable).IsAssignableFrom(context.ObjectType))
            {
                // todo : récupérer le count si ISelection
                metadata = new Metadata(entitySerializer.SerializeEntities((IEnumerable)context.Object, query.Fields), query.Options, query.Page, execution);
            }
            else if (typeof(ISelection).IsAssignableFrom(context.ObjectType))
            {
                metadata = new Metadata(entitySerializer.SerializeSelection((ISelection)context.Object, query), query.Options, query.Page, execution);
            }
            else
            {
                metadata = new Metadata(entitySerializer.SerializeEntity(context.Object, query.Fields), query.Options, query.Page, execution);
            }

            var output = JsonConvert.SerializeObject(metadata.ToDictionary(), serializerSettings);
            context.HttpContext.Response.ContentType = "application/json";
            return context.HttpContext.Response.WriteAsync(output);
        }
    }
}
