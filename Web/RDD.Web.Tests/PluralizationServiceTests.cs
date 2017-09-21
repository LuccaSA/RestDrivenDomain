using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Infra.Contexts;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Tests.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace RDD.Web.Tests
{
	public class PluralizationServiceTests
	{
		private PluralizationService _service;

		public PluralizationServiceTests()
		{
			_service = new PluralizationService();
		}

		[Theory]
		[InlineData("legalEntity", "legalEntities")]
		[InlineData("user", "users")]
		[InlineData("employee", "employees")]
		public void Plurals_should_work(string singular, string plural)
		{
			var result = _service.GetPlural(singular);

			Assert.Equal(plural, result);
		}
	}
}
