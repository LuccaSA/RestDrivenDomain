using RDD.Domain.Models;
using System;

namespace RDD.Domain.Tests.Models
{
	public class UsersCollection : RestCollection<User, int>
	{
		public UsersCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage)
			: base(storage, execution, combinationsHolder, asyncStorage) { }
	}
}
