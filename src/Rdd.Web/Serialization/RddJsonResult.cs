﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Providers;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization
{
    internal static class RddJsonResult
    {
        public static readonly string DefaultContentType = new MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString();
    }

    public class RddJsonResult<T> : JsonResult
        where T : class
    {
        public IExpressionTree Fields { get; private set; }

        public RddJsonResult(T value, IExpressionTree fields)
            : base(value)
        {
            Fields = fields;
        }

        public RddJsonResult(ISelection<T> value, IExpressionTree fields)
            : base(value)
        {
            Fields = fields;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;

            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(ContentType, response.ContentType, RddJsonResult.DefaultContentType,
                out var resolvedContentType, out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (StatusCode != null)
            {
                response.StatusCode = StatusCode.Value;
            }

            var services = context.HttpContext.RequestServices;
            using (var writer = services.GetRequiredService<IHttpResponseStreamWriterFactory>().CreateWriter(response.Body, resolvedContentTypeEncoding))
            {
                return WriteResult(services, writer, DateTime.Now);
            }
        }

        private string GetPrincipalName(IServiceProvider services)
        {
            var principal = services.GetService<ClaimsPrincipal>();
            if (principal == null)
            {
                return null;
            }
            var name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (name == null)
            {
                return null;
            }

            return name.Value;
        }

        internal async Task WriteResult(IServiceProvider services, TextWriter writer, DateTime generatedAt)
        {
            Value = new Metadata(Value, GetPrincipalName(services), generatedAt);

            using (var jsonWriter = new JsonTextWriter(writer) { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified })
            {
                jsonWriter.ArrayPool = new JsonArrayPool<char>(services.GetRequiredService<ArrayPool<char>>());
                jsonWriter.CloseOutput = false;
                jsonWriter.AutoCompleteOnClose = false;

                services.GetRequiredService<ISerializerProvider>().WriteJson(jsonWriter, Value, Fields);
            }

            // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
            // buffers. This is better than just letting dispose handle it (which would result in a synchronous write).
            await writer.FlushAsync();

        }
    }
}