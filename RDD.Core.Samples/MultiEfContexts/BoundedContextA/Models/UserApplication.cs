using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Rights;
using RDD.Samples.MultiEfContexts.BoundedContextA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.BoundedContextA.Models
{
	public class UserApplication : Application
	{
		public UserApplication()
			:base()
		{
			Id = "USERS";
			Name = "User management system";

			var read = new Operation(id: 1, code: "UserRead", name: "Read users");

			Operations.Add(read);

			Combinations.Add(new Combination { Application = this, EntityType = typeof(User), Operation = read, Verb = HttpVerb.GET });
		}
	}
}
