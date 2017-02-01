using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
	public class SerializationServiceTests
	{
		[Fact]
		public void SerializeStringAsMailAddressWhenGoodMailInQueryFilters()
		{
			var service = new SerializationService();
			var values = service.ConvertWhereValues(new HashSet<string>() { "mail@domain.com" }, typeof(User).GetProperty("Mail"));
		}
	}
}
