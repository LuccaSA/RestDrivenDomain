using System;
using System.Linq;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RDD.Domain.Models.Querying;
using RDD.Web.Middleware;
using RDD.Web.Models;

namespace RDD.Web.Serialization
{
    /// <summary>
    ///     Used to track the serialisation tree walkthrough
    /// </summary>
    public class SelectiveSerialisationContext
    {
        private static readonly AsyncLocal<SelectiveSerialisationContext> _context = new AsyncLocal<SelectiveSerialisationContext>();
        private readonly bool _serializeEverything;
        private readonly Stack<Node> _stack;

        public SelectiveSerialisationContext(Node root)
        {
            if (root == null)
            {
                _serializeEverything = true;
            }
            else
            {
                _stack = new Stack<Node>();
                _stack.Push(root);
                CurrentNode = root;
            }
        }

        public static SelectiveSerialisationContext Current
        {
            get => _context.Value;
            set => _context.Value = value;
        }

        public Node CurrentNode { get; private set; }
        public Node CurrentPropertyNode { get; private set; }

        public void Push()
        {
            if (CurrentPropertyNode != null)
            {
                _stack.Push(CurrentPropertyNode);
                CurrentNode = CurrentPropertyNode;
                CurrentPropertyNode = null;
            }
        }

        public void Pop()
        {
            if (_stack == null || _stack.Count == 0)
            {
                return;
            }
            CurrentPropertyNode = _stack.Pop();
            CurrentNode = CurrentPropertyNode.ParentNode;
        }

        public bool IsCurrentNodeDefined(string propertyName)
        {
            if (_serializeEverything)
            {
                return true;
            }
            if (CurrentNode.Children.TryGetValue(propertyName, out var propNode))
            {
                CurrentPropertyNode = propNode;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Selective JsonOutputFormatter letting choose which properties you want to serialize
    /// </summary>
    public class SelectiveJsonOutputFormatter : JsonOutputFormatter
    {
        private readonly IArrayPool<char> _charPool;

        public SelectiveJsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool)
            : base(serializerSettings, charPool)
        {
            _charPool = new JsonArrayPool<char>(charPool);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            object payload = PreparePayload(context, out Node node);

            var response = context.HttpContext.Response;
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                using (var jsonWriter = new TrackedJsonTextWriter(writer, node)
                {
                    ArrayPool = _charPool,
                    CloseOutput = false,
                    AutoCompleteOnClose = false
                })
                {
                    var jsonSerializer = CreateJsonSerializer();
                    jsonSerializer.Serialize(jsonWriter, payload);
                }

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous
                // write).
                await writer.FlushAsync();
            }
        }

        protected virtual object PreparePayload(OutputFormatterWriteContext context, out Node node)
        {
            if (context.HttpContext.Request.Query.TryGetValue(QueryTokens.Fields, out var fieldValues))
            {
                node = ParseFields(fieldValues);
            }
            else
            {
                node = null;
            }

            return context.Object;
        }

        protected virtual Node ParseFields(StringValues values)
        {
            return values.SelectMany(QueryExpansionHelper.Expand).ParseNode();
        }
    }

    /// <summary>
    /// Encapsulates a selective Serialized Json with metadata 
    /// </summary>
    public class MetaSelectiveJsonOutputFormatter : SelectiveJsonOutputFormatter
    {
        public MetaSelectiveJsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool)
            : base(serializerSettings, charPool)
        {
        }

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
                items.Children = node.Children;
                node.ParentNode = items;

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
                items.Children = node.Children;
                node.ParentNode = items;

                return new Meta()
                {
                    Header = metaHeader,
                    Data = data
                };
            }
        }
    }

    internal static class OutputFormatterExtensions
    {
        public static T GetService<T>(this OutputFormatterWriteContext context)
        {
            return (T)context.HttpContext.RequestServices.GetService(typeof(T));
        }
    }


}