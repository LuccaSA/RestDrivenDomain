using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using RDD.Domain.Services;
using RDD.Infra;
using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Querying;
using RDD.Infra.Models.Rights;
using RDD.Infra.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.SharedKernel.Services
{
	public class AppInstancesService : SampleRestService<AppInstance, int>, IAppInstancesService
	{
		public AppInstancesService(IStorageService storage, IExecutionContext execution, string appTag = "")
			: base(storage, execution, appTag)
		{
		}

		protected override IAppInstance GetAppInstanceByTag(string appTag) { return Set().Where(i => i.ApplicationID == "ADMIN").FirstOrDefault(); }
		protected override IAppInstance GetAppInstanceById(int appInstanceID) { return Set().Where(i => i.ApplicationID == "ADMIN").FirstOrDefault(); }

		protected override List<AppInstance> Prepare(List<AppInstance> entities, Query<AppInstance> query)
		{
			var apps = RestServiceProvider.Get<Application, string>(_storage, _execution);

			foreach (var entity in entities)
			{
				entity.Application = apps.GetById(entity.ApplicationID);
			}

			return entities;
		}

		public IAppInstance GetInstanceByTag<TEntity>(string tag)
		{
			tag = tag ?? "";

			return GetInstanceByCondition<TEntity>(a => a.Tag == tag, tag);
		}

		public IAppInstance GetInstanceById<TEntity>(int id)
		{
			return GetInstanceByCondition<TEntity>(a => a.Id == id, id);
		}

		public bool HasApplicationInstance(string applicationID)
		{
			return Set().Any(i => i.ApplicationID == applicationID);
		}

		private IAppInstance GetInstanceByCondition<TEntity>(Expression<Func<AppInstance, bool>> condition, object value)
		{
			var app = RestServiceProvider.Get<Application, string>(_storage, _execution).Get(a => a.Combinations.Any(c => c.EntityType.IsAssignableFrom(typeof(TEntity)))).FirstOrDefault();

			if (app != null)
			{
				//Ici on ne peut pas utiliser le Get() standard car ça relancerait un SetAppInstance en boucle !
				var appInstance = Set().Where(i => i.ApplicationID == app.Id).Where(condition).FirstOrDefault();

				if (appInstance != null)
				{
					var list = Prepare(new List<AppInstance>() { appInstance }, new Query<AppInstance>());

					return list.FirstOrDefault();
				}

				throw new Exception(String.Format("No appInstance that matches '{0}'", (value ?? "").ToString()));
			}

			throw new Exception(String.Format("Unreachable entity Type '{0}'", typeof(TEntity).Name));
		}

		public List<Operation> GetOperations<TEntity>(IAppInstance instance, HttpVerb verb)
		{
			return instance.Application.Combinations
				.Where(c => c.EntityType.IsAssignableFrom(typeof(TEntity)) && c.Verb == verb)
				.Select(c => c.Operation).ToList();
		}

		public List<Operation> GetAllOperations<TEntity>(IAppInstance instance)
		{
			return instance.Application.Combinations.Where(c => c.EntityType == typeof(TEntity))
				.Select(c => c.Operation).Distinct().ToList();
		}
	}
}
