using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models
{
	public class User : EntityBase<User, int>
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
	}
}
