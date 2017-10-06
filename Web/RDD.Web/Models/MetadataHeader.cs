using RDD.Domain;
using System;

namespace RDD.Web.Models
{
	public class MetadataHeader
	{
		public DateTime generated { get; set; }

		public long queryTime { get; set; }

		public string principal { get; set; }

		public MetadataPaging Paging { get; set; }

		public MetadataHeader(IExecutionContext execution)
		{
			queryTime = execution.queryWatch.ElapsedMilliseconds;

			principal = execution.curPrincipal.Name;
		}
	}

}
