using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public class Combination
	{
		public Application Application { get; set; }
		public Operation Operation { get; set; }
		public HttpVerb Verb { get; set; }

		public Type Subject { get; set; }
	}
}
