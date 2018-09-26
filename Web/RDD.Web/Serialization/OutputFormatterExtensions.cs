using Microsoft.AspNetCore.Mvc.Formatters;

namespace RDD.Web.Serialization
{
    internal static class OutputFormatterExtensions
    {
        public static T GetService<T>(this OutputFormatterWriteContext context)
        {
            return (T)context.HttpContext.RequestServices.GetService(typeof(T));
        }
    }
}