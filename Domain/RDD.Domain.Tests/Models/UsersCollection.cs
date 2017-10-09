using RDD.Domain.Models;

namespace RDD.Domain.Tests.Models
{
	public class UsersCollection : RestCollection<User, int>
	{
		public UsersCollection(IRepository<User> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
			: base(repository, execution, combinationsHolder) { }
	}
}
