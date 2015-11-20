using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Models
{
	public class Metadata
	{
		public MetadataHeader header { get; set; }

		public object data { get; set; }

		public Metadata(object data)
		{
			header = new MetadataHeader { generated = DateTime.Now };
			this.data = data;
		}

		public Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				{"header", header},
				{"data", data}
			};
		}
	}
}