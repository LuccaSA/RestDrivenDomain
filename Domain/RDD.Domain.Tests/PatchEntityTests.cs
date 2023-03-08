﻿using NUnit.Framework;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
	public class PatchEntityTests
	{
		[Test]
		public void Uri_should_accept_string_in_json()
		{
			var json = @"{ ""twitterUri"": ""https://twitter.com"" }";
			var user = new User();
			var patcher = new PatchEntityHelper(null);
			
			patcher.PatchEntity(user, PostedData.ParseJSON(json));
		}
	}
}