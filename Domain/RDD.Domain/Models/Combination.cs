using RDD.Domain.Helpers;
using System;

namespace RDD.Domain.Models
{
    public class Combination
    {
        public Application Application { get; set; }
        public Operation Operation { get; set; }
        public HttpVerbs Verb { get; set; }

        public Type Subject { get; set; }
    }
}
