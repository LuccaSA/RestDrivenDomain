using RDD.Application.Controllers;
using RDD.Domain.Models;
using System;

namespace RDD.Domain.Tests.Models
{
	public class UsersAppController : AppController<UsersCollection, User, int>
	{
		public UsersAppController(IStorageService storage, UsersCollection collection)
			: base(storage, collection) { }
	}
}
