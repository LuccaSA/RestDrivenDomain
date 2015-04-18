using RDD.Infra.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Rights
{
	public class Combination
	{
		public IApplication Application { get; set; }
		public Operation Operation { get; set; }
		public HttpVerb Verb { get; set; }
		public Type EntityType { get; set; }
	}
}
