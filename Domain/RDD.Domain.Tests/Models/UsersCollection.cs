using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests.Models
{
	public class UsersCollection : RestCollection<User, int>
	{
		public UsersCollection(IStorageService storage, IExecutionContext execution, Func<IStorageService> asyncStorage)
			: base(storage, execution, asyncStorage) { }
	}
}
