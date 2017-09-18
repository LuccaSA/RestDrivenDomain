using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDD.Web.Tests.Models
{
	public class User : EntityBase<User, int>
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
	}
}
