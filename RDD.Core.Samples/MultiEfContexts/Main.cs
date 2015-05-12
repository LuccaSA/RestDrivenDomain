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
using Microsoft.Practices.Unity;

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
					var unity = new UnityContainer();

					unity.RegisterType<IApplicationsService, ApplicationsService>(new InjectionConstructor(storage, execution));

					var applications = (RestDomainService<Application, string>)unity.Resolve<IApplicationsService>();
					
					unity.RegisterType<IAppInstancesService, AppInstancesService>(new InjectionConstructor(storage, execution, applications));

					var appInstances = (RestDomainService<AppInstance, int>)unity.Resolve<IAppInstancesService>();

					unity.RegisterType<IUsersService, UsersService>(new InjectionConstructor(storage, execution, appInstances));

					var userApp = new UserApplication();
					var userAppInstance = new AppInstance { Id = 1, Name = "UsersManagementSystem", Application = userApp, ApplicationID = userApp.Id };
					unity.Resolve<IApplicationsService>().RegisterApplication(new UserApplication());
					appInstances.Create(userAppInstance);

					var userA = new User { Id = 1, FirstName = "Jean", LastName = "Dupont" };
					var userB = new User { Id = 2, FirstName = "Paul", LastName = "Martin" };

					var users = unity.Resolve<IUsersService>();

					users.Create(userA);
					users.Create(userB);

					storage.Commit();
				}
			}
		}
	}
}
