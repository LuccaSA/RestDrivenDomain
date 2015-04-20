using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDD.Infra.Providers;
using RDD.Samples.MultiEfContexts.BoundedContextA;
using RDD.Samples.MultiEfContexts.BoundedContextA.Services;
using RDD.Samples.Common;
using RDD.Infra;
using RDD.Domain.Services;
using RDD.Core.Samples.MultiEfContexts.BoundedContextA.Models;

namespace RDD.Samples.MultiEfContexts
{
	[TestClass]
	public class Main
	{
		[TestMethod]
		public void Run()
		{
			using (var execution = new ExecutionContext())
			{
				using (var storage = new EntityContext())
				{
					RestServiceProvider.Register<IApplication, string>((storage_, execution_) => new RestService<AppInstance, IAppInstance, int>(storage_, execution_));
					RestServiceProvider.Register<IAppInstance, int>((storage_, execution_) => new RestService<AppInstance, IAppInstance, int>(storage_, execution_));
					RestServiceProvider.Register<IUser, int>((storage_, execution_) => new UsersService(storage_, execution_));

					var userA = new User { Id = 1, FirstName = "Jean", LastName = "Dupont" };
					var userB = new User { Id = 2, FirstName = "Paul", LastName = "Martin" };

					var users = RestServiceProvider.Get<IUser, int>(storage, execution);

					users.Create(userA);
					users.Create(userB);

					storage.Commit();
				}
			}
		}
	}
}
