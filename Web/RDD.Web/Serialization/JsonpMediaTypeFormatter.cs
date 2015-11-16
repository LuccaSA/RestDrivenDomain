using RDD.Domain.Models.Querying;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace RDD.Web.Serialization
{
	/// <summary>
	/// http://stackoverflow.com/questions/9421312/jsonp-with-asp-net-web-api
	/// </summary>
	public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
	{
		private string callbackQueryParameter;

		public JsonpMediaTypeFormatter()
		{
			SupportedMediaTypes.Add(DefaultMediaType);
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/javascript"));

			MediaTypeMappings.Add(new UriPathExtensionMapping("jsonp", DefaultMediaType));
		}

		public string CallbackQueryParameter
		{
			get { return callbackQueryParameter ?? Reserved.callback.ToString(); }
			set { callbackQueryParameter = value; }
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			string callback;

			if (IsJsonpRequest(out callback))
			{
				return Task.Factory.StartNew(() =>
				{
					var writer = new StreamWriter(writeStream);
					writer.Write(callback + "(");
					writer.Flush();

					base.WriteToStreamAsync(type, value, writeStream, content, transportContext).Wait();

					writer.Write(");");
					writer.Flush();
				});
			}
			else
			{
				return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
			}
		}

		private bool IsJsonpRequest(out string callback)
		{
			callback = null;

			if (HttpContext.Current.Request.HttpMethod != "GET")
				return false;

			callback = HttpContext.Current.Request.QueryString[CallbackQueryParameter];

			return !string.IsNullOrEmpty(callback);
		}
	}
}