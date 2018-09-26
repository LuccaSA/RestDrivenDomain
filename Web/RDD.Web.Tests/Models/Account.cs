using System.Collections.Generic;

namespace RDD.Web.Tests.Models
{
    public class Account
    {
        public string Name { get; set; }

        public List<Rule> Rules { get; set; }
    }
}