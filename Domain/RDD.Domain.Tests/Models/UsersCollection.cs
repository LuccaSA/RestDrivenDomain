using RDD.Domain.Contracts;
using RDD.Domain.Models.Collections;
using RDD.Domain.Models.Convertors;
using System.Diagnostics;

namespace RDD.Domain.Tests.Models
{
	public class UsersCollection : RestCollection<User, int>
	{
		public UsersCollection(Stopwatch queryWatch, IRepository<User> repository, IQueryConvertor<User> convertor)
			: base(queryWatch, repository, convertor) { }
	}
}
