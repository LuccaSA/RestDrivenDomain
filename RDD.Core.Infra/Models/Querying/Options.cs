using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	public class Options
	{
		public bool NeedCount { get; set; }
		public bool NeedEnumeration { get; set; }

		public bool withPaging { get; set; }
		public Page Page { get; set; }
		public bool withMetadata { get; set; }
		public bool attachOperations { get; set; }
		public bool attachActions { get; set; }

		public PostedData FilterOperations { get; set; }

		public Options()
		{
			NeedEnumeration = true;
		}
	}
}