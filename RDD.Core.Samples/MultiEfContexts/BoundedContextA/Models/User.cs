using RDD.Infra;
using RDD.Infra.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.BoundedContextA
{
	public class User : EntityBase<User, int>, IUser
	{
		public override int Id { get; set; }
		public override string Name { get { return FirstName + " " + LastName; } set { throw new NotImplementedException(); } }
		public string LastName { get; set; }
		public string FirstName { get; set; }
	}
}
