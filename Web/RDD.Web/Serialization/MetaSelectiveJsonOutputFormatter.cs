using System;
using System.Buffers;
using System.Collections;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
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

        protected override object PreparePayload(OutputFormatterWriteContext context, out Node node)
        {
            var data = base.PreparePayload(context, out node);

            QueryContext queryContext = context.GetService<QueryContext>();

            var metaHeader = new MetaHeader()
            {
                Generated = queryContext.EllapsedTime(),
                Paging = new MetaPaging
                {
                    Count = queryContext.Response.TotalCount,
                    Offset = queryContext.Request.PageOffset * queryContext.Request.ItemPerPage
                }
            };

            // in case of collection, returned json have a specific Items field
            if (context.ObjectType.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                // include the Meta paths, with Items
                Node root = Node.NewRoot();
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
                    items.Children = node.Children;
                    foreach (var n in items.Children.Values)
                    {
                        n.ParentNode = items;
                    }
                }
                
                node = root;

                // in case of 
                return new Meta()
                {
                    Header = metaHeader,
                    Data = new MetaItems { Items = data }
                };
            }
            else
            {
                // include the Meta paths, with Data
                Node root = Node.NewRoot();
                root.GetOrCreateChildNode("Header");
                var items = root.GetOrCreateChildNode("Data");
                // If no node structure defined, the whole Data is serialiazed
                if (node != null)
                {
                    items.Children = node.Children;
                    foreach (var n in items.Children.Values)
                    {
                        n.ParentNode = items;
                    }
                }
                node = root;
                return new Meta()
                {
                    Header = metaHeader,
                    Data = data
                };
            }
        }
    }
}