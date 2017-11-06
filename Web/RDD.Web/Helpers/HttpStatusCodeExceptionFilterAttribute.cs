using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RDD.Domain.Exceptions;

namespace RDD.Web.Helpers
{
    public class HttpStatusCodeExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpStatusCodeExceptionMiddleware> _logger;

        public HttpStatusCodeExceptionMiddleware(RequestDelegate next, ILogger<HttpStatusCodeExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
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
                    _logger.LogWarning("The response has already started, the http status code middleware will not be executed.");
                    throw;
                }

                if (ex is IStatusCodeException eStatus)
                {
                    int httpCode = (int) eStatus.StatusCode;
                    context.Response.Clear();
                    context.Response.StatusCode = httpCode;
                    context.Response.ContentType = "application/json";

                    // We provide detailled logs only with functional exceptions
                    if (httpCode >= 400 && httpCode < 500)
                    {
                        await context.Response.WriteAsync(ex.Message);
                    }
                }
                else
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                }
            }
        }
    }
     
}