using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.BoundedContextA
{
	public interface IUsersService : IRestDomainService<User, int>
	{
	}
}
