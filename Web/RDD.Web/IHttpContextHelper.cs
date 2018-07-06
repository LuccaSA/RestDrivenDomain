using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace RDD.Web
{
    public interface IHttpContextHelper
    {
        string GetContentType();
        IDictionary<string, StringValues> GetHeaders();
        string GetContent();
        Dictionary<string, string> GetQueryNameValuePairs();
    }
}
