﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rdd.Domain.Exceptions;

namespace Rdd.Web.Helpers
{
    public class HttpStatusCodeExceptionMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<ExceptionHttpStatusCodeOption> _options;
        private readonly RequestDelegate _next;  

        public HttpStatusCodeExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<ExceptionHttpStatusCodeOption> options)
        {
            _loggerFactory = loggerFactory;
            _options = options;
            _next = next ?? throw new ArgumentNullException(nameof(next));  
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    var logger = _loggerFactory.CreateLogger<HttpStatusCodeExceptionMiddleware>();
                    logger.LogWarning("The response has already started, the http status code middleware will not be executed.");
                    throw;
                }

                if (ex is IStatusCodeException eStatus)
                {
                    await StatusCodeExceptionResponse(context, ex, eStatus);
                }
                else
                {
                    StandardExceptionResponse(context, ex);
                }
            }
        }

        private void StandardExceptionResponse(HttpContext context, Exception ex)
        {
            HttpStatusCode? overridenStatus = null;
            if (_options?.Value?.StatusCodeMapping != null)
            {
                overridenStatus = _options?.Value?.StatusCodeMapping(ex);
            }
            context.Response.Clear();
            context.Response.StatusCode = (int)(overridenStatus ?? HttpStatusCode.BadRequest);
            context.Response.ContentType = "application/json";
        }

        private static async Task StatusCodeExceptionResponse(HttpContext context, Exception ex, IStatusCodeException eStatus)
        {
            var httpCode = eStatus.StatusCode;
            context.Response.Clear();
            context.Response.StatusCode = (int)httpCode;
            context.Response.ContentType = "application/json";

            // We provide detailled logs only with functional exceptions
            if (httpCode >= HttpStatusCode.BadRequest && httpCode < HttpStatusCode.InternalServerError)
            {
                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}