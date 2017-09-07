using RDD.Domain.Models.Querying;
using RDD.Web.Contexts;
using RDD.Web.QueryParsers.Includes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.QueryParsers
{
	class QueryParser<T> : IQueryParser<T>
	{
		protected HashSet<string> IgnoredFilters { get; set; }

		public QueryParser()
		{
			IgnoredFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		public void IgnoreFilters(params string[] filters)
		{
			foreach (var filter in filters)
			{
				IgnoredFilters.Add(filter);
			}
		}

		public Query<T> ParseWebContext(IWebContext webContext, bool isCollectionCall)
		{
			var parameters = webContext.GetQueryNameValuePairs().Where(v => !IgnoredFilters.Contains(v.Key)).ToDictionary(K => K.Key.ToLower(), K => K.Value);
			var fields = new FieldsParser<T>().ParseFields(parameters, isCollectionCall);

			return new Query<T>
			{
				Fields = fields,
				Filters = Where.Parse<T>(parameters),
				Headers = new HeadersParser().Parse(webContext.Headers),
				Includes = new IncludeParser<T>().ParseIncludes(parameters, isCollectionCall),
				OrderBys = new OrderByParser().Parse(parameters),
				Options = new OptionsParser().Parse<T>(parameters, fields)
			};
		}
	}
}