using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Web.Models
{
	public class Metadata
	{
		public MetadataHeader Header { get; set; }

		public object Data { get; set; }

		public Metadata(object datas, Options options, IExecutionContext execution)
		{
			Header = new MetadataHeader(execution) { generated = DateTime.Now };
			Data = datas;

			if (options.withPagingInfo)
			{
				var offset = options.Page.Offset;
				var limit = options.Page.Limit;
				var count = options.Page.TotalCount;

				string next = null;
				string previous = null;

				if (offset + limit < count)
				{
					//var nextOffset = offset + limit;
					//var url = HttpContext.Current.Request.Url;

					//next = BuildUrlFromOffsetLimitAndQueryParams(url, nextOffset, limit, queryParameters);
				}

				if (offset > 0)
				{
					//var previousOffset = Math.Max(0, offset - limit);
					//var url = HttpContext.Current.Request.Url;

					//previous = BuildUrlFromOffsetLimitAndQueryParams(url, previousOffset, limit, queryParameters);
				}

				Header.Paging = new MetadataPaging() { count = count, offset = offset, limit = limit, next = next, previous = previous };
			}
		}

		public Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				{"header", Header},
				{"data", Data}
			};
		}
	}
}