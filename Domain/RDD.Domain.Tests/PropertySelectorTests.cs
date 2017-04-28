using RDD.Domain.Helpers;
using RDD.Domain.Models;
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
	class PropertySelectorTests
	{
		[Fact]
		public void Parsing_count_on_empty_collection()
		{
			var field = "count";
			var selector = new CollectionPropertySelector<User>();
			selector.Parse(field);
		}

		[Fact]
		public void Replace_child_and_return_false_WHEN_adding_subs_on_existing_child()
		{
			var selector = new PropertySelector<User>(u => u.Id, u => u.Name, u => u.Mail);
			var subSelector = new PropertySelector<MailAddress>(m => m.DisplayName, m => m.Host);

			var result = selector.Add<MailAddress>(subSelector, (Expression<Func<User, MailAddress>>)(u => u.Mail));

			Assert.Equal(false, result);
			Assert.True(selector.Contains(u => u.Name));
			Assert.True(selector.Contains(u => u.Mail));
			Assert.True(selector.Contains(u => u.Mail.Host));
		}
	}
}
