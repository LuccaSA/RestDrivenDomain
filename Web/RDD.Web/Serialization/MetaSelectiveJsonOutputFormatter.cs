using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using RDD.Domain;
using RDD.Domain.Helpers;
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
            if (data == null)
            {
                return null;
            }

            var queryMetadata = context.GetService<QueryMetadata>();
            
            // in case of collection, returned json have a specific Items field
            var root = PropertyTreeNode.NewRoot();

            bool isCollection = context.ObjectType.GenericTypeArguments.Length != 0
                                && context.ObjectType.GenericTypeArguments[0].IsClass
                                && context.ObjectType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (isCollection)
            {
                // include the Meta paths, with Items
                root.GetOrCreateChildNode("Header");
                var items = root.GetOrCreateChildNode("Data")
                    .GetOrCreateChildNode("Items");

                if (node == null)
                {
                    // If no node structure defined, fallback with Id / Name for collections
                    // ONLY for Get verb, for retro-compatibility reason
                    if (string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        items.GetOrCreateChildNode("Id");
                        items.GetOrCreateChildNode("Name");
                    }
                }
                else
                {
                    node.Reparent(items);
                }

                node = root;

                var metaHeader = new MetaHeaderWithPaging
                {
                    Generated = queryMetadata.EllapsedTime(),
                    Principal = context.HttpContext?.User?.Identity?.Name
                };

                if (queryMetadata.Paging != null)
                {
                    metaHeader.Paging = new MetaPaging
                    {
                        Count = queryMetadata.TotalCount,
                        Offset = queryMetadata.Paging.PageOffset * queryMetadata.Paging.ItemPerPage
                    };
                }

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
                    Header = new MetaHeader
                    {
                        Generated = queryMetadata.EllapsedTime(),
                        Principal = context.HttpContext?.User?.Identity?.Name
                    },
                    Data = data
                };
            }
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context) => context.ContentType == MetaDataContentType;
    }
}