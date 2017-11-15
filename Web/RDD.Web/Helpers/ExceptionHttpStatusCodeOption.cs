using System;
using System.Net;

namespace RDD.Web.Helpers
{
    public class ExceptionHttpStatusCodeOption
    {
        public Func<Exception, HttpStatusCode?> StatusCodeMapping { get; set; }
    }
}