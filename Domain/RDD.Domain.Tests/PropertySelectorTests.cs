using NUnit.Framework;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
	class PropertySelectorTests
	{
		[Test]
		public void Parsing_count_on_empty_collection()
		{
			var field = "count";
			var selector = new CollectionPropertySelector<User>();
			selector.Parse(field);
		}
	}
}
