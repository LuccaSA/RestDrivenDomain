using System;
using Newtonsoft.Json;
using RDD.Domain;
using RDD.Domain.Contexts;

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
			var execution = Resolver.Current().Resolve<IExecutionContext>();

			execution.serverWatch.Stop();

			serverTime = execution.serverWatch.ElapsedMilliseconds;
			queryTime = execution.queryWatch.ElapsedMilliseconds;

			principal = execution.curPrincipal.Name;
		}
	}

}
