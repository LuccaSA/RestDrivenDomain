using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using Xunit;

namespace RDD.Domain.Tests
{
	public class PatchEntityTests
	{
		[Fact]
		public void Uri_SHOULD_accept_string_in_json()
		{
			var json = @"{ ""twitterUri"": ""https://twitter.com"" }";
			var user = new User();
			var patcher = new PatchEntityHelper();
			
			patcher.PatchEntity(user, PostedData.ParseJSON(json));
		}
	}
}
