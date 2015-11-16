using System;
using Newtonsoft.Json;
using RDD.Domain;

namespace RDD.Web.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MetadataHeader
	{
		[JsonProperty]
		public DateTime generated { get; set; }

		[JsonProperty]
		public long serverTime { get; set; }

		[JsonProperty]
		public long queryTime { get; set; }

		[JsonProperty]
		public string principal { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public MetadataPaging paging { get; set; }

		public MetadataHeader()
		{
			//_execution.serverWatch.Stop();

			//serverTime = _execution.serverWatch.ElapsedMilliseconds;
			//queryTime = _execution.queryWatch.ElapsedMilliseconds;

			//principal = _execution.curPrincipal.Name;
		}
	}

}
