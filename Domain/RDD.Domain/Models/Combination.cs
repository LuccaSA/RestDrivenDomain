using Rdd.Domain.Helpers;
using System;

namespace Rdd.Domain.Models
{
    public class Combination
    {
        public Operation Operation { get; set; }
        public HttpVerbs Verb { get; set; }

        public Type Subject { get; set; }
    }
}
