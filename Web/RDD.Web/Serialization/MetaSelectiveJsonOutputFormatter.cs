using System;
using System.Buffers;
using System.Collections;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Encapsulates a selective Serialized Json with metadata 
    /// </summary>
    public class MetaSelectiveJsonOutputFormatter : SelectiveJsonOutputFormatter
    {
        public MetaSelectiveJsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool)
            : base(serializerSettings, charPool)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(MetaDataContentType);
        }

        public const String MetaDataContentType = "application/select.metadata+json";

        protected override object PreparePayload(OutputFormatterWriteContext context, out PropertyTreeNode node)
        {
            var data = base.PreparePayload(context, out node);

            var queryMetadata = context.GetService<QueryMetadata>();

            var metaHeader = new MetaHeader()
            {
                Generated = queryMetadata.EllapsedTime(),
                Paging = new MetaPaging
                {
                    Count = queryMetadata.TotalCount,
                    Offset = queryMetadata.Paging.PageOffset * queryMetadata.Paging.ItemPerPage
                }
            };

            // in case of collection, returned json have a specific Items field
            var root = PropertyTreeNode.NewRoot();
            if (context.ObjectType.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                // include the Meta paths, with Items
                root.GetOrCreateChildNode("Header");
                var items = root.GetOrCreateChildNode("Data")
                    .GetOrCreateChildNode("Items");

                // If no node structure defined, fallback with Id / Name for collections
                if (node == null)
                { 
                    items.GetOrCreateChildNode("Id");
                    items.GetOrCreateChildNode("Name");
                }
                else
                {
                    node.Reparent(items);
                }

                node = root;

                return new Meta
                {
                    Header = metaHeader,
                    Data = new MetaItems { Items = data }
                };
            }
            else
            {
                // include the Meta paths, with Data
                root.GetOrCreateChildNode("Header");
                var items = root.GetOrCreateChildNode("Data");
                
                // If no node structure defined, the whole Data is serialiazed
                node?.Reparent(items);
                node = root;

                return new Meta
                {
                    Header = metaHeader,
                    Data = data
                };
            }
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context) => context.ContentType == MetaDataContentType;
    }
}