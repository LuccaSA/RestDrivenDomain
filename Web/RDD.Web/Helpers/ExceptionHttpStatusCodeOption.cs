using System;
using System.Net;

namespace Rdd.Web.Helpers
{
    public class ExceptionHttpStatusCodeOption
    {
        public Func<Exception, HttpStatusCode?> StatusCodeMapping { get; set; }
    }
}