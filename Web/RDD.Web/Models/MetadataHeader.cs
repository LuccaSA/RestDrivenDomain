using System;
using Newtonsoft.Json;
using RDD.Domain;

namespace RDD.Web.Models
{
	public class MetadataHeader
	{
		public DateTime generated { get; set; }

		public long serverTime { get; set; }

		public long queryTime { get; set; }

		public string principal { get; set; }

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
