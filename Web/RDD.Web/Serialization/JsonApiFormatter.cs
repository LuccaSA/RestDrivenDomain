using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;

namespace RDD.Web.Serialization
{
	public class JsonApiFormatter
	{
		public static JsonMediaTypeFormatter GetInstance(IWebContext webContext, IContractResolver resolver)
		{
			var formatter = webContext.GetQueryNameValuePairs().ContainsKey(Reserved.callback.ToString()) ? new JsonpMediaTypeFormatter() : new JsonMediaTypeFormatter();
			formatter.SerializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = resolver };
			return formatter;
		}
	}
}