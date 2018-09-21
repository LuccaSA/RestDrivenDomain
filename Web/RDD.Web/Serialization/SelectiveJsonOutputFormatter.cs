﻿using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Querying;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Selective JsonOutputFormatter letting choose which properties you want to serialize
    /// </summary>
    public class SelectiveJsonOutputFormatter : JsonOutputFormatter
    {
        private readonly IArrayPool<char> _charPool;

        public const String SelectiveContentType = "application/select+json";

        public SelectiveJsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool)
            : base(serializerSettings, charPool)
        {
            _charPool = new JsonArrayPool<char>(charPool);
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(SelectiveContentType);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            return WriteResponseBodyInternalAsync(context, selectedEncoding);
        }

        private async Task WriteResponseBodyInternalAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            object payload = PreparePayload(context, out PropertyTreeNode node);
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

        protected virtual object PreparePayload(OutputFormatterWriteContext context, out PropertyTreeNode node)
        {
            node = context.HttpContext.ParseFields();
            return context.Object;
        }
    }
}