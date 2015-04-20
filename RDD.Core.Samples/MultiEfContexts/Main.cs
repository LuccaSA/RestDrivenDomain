using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDD.Infra.Providers;
using RDD.Samples.MultiEfContexts.BoundedContextA;
using RDD.Samples.MultiEfContexts.BoundedContextA.Services;
using RDD.Samples.Infra;
using RDD.Infra;
using RDD.Domain.Services;
using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using RDD.Samples.MultiEfContexts.SharedKernel.Services;
using RDD.Infra;
using RDD.Samples.MultiEfContexts.BoundedContextA.Models;
using RDD.Samples.MultiEfContexts.SharedKernel;

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
				using (var storage = new StorageService())
				{
					RestServiceProvider.Register<Application, string>((storage_, execution_, appTag_) => new ApplicationsService(storage_, execution_));
					RestServiceProvider.Register<AppInstance, int>((storage_, execution_, appTag_) => new AppInstancesService(storage_, execution_));
					RestServiceProvider.Register<User, int>((storage_, execution_, appTag_) => new UsersService(storage_, execution_));

					var userApp = new UserApplication();
					var userAppInstance = new AppInstance { Id = 1, Name = "UsersManagementSystem", Application = userApp, ApplicationID = userApp.Id };
					((IApplicationsService)RestServiceProvider.Get<Application, string>(storage, execution)).RegisterApplication(new UserApplication());
					((RestDomainService<AppInstance, int>)RestServiceProvider.Get<AppInstance, int>(storage, execution)).Create(userAppInstance);

					var userA = new User { Id = 1, FirstName = "Jean", LastName = "Dupont" };
					var userB = new User { Id = 2, FirstName = "Paul", LastName = "Martin" };

					var users = (RestDomainService<User, int>)RestServiceProvider.Get<User, int>(storage, execution);

					users.Create(userA);
					users.Create(userB);

					storage.Commit();
				}
			}
		}
	}
}
