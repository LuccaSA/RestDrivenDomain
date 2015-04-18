using RDD.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.BoundedContextA
{
	public interface IUser : IEntityBase<int>
	{
		string LastName { get; set; }
		string FirstName { get; set; }
	}
}
