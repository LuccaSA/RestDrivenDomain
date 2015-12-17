using NUnit.Framework;
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

		[Test]
		public void Replace_child_and_return_false_when_adding_subs_on_existing_child()
		{
			var selector = new PropertySelector<User>(u => u.Id, u => u.Name, u => u.Mail);
			var subSelector = new PropertySelector<MailAddress>(m => m.DisplayName, m => m.Host);

			var result = selector.Add<MailAddress>(subSelector, (Expression<Func<User, MailAddress>>)(u => u.Mail));

			Assert.AreEqual(false, result);
			Assert.IsTrue(selector.Contains(u => u.Name));
			Assert.IsTrue(selector.Contains(u => u.Mail));
			Assert.IsTrue(selector.Contains(u => u.Mail.Host));
		}
	}
}
