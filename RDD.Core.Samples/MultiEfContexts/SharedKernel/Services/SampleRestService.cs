using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using RDD.Domain.Helpers;
using RDD.Domain.Services;
using RDD.Infra;
using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Querying;
using RDD.Infra.Models.Rights;
using RDD.Infra.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.SharedKernel.Services
{
	public class SampleRestService<TEntity, TKey> : RestDomainService<TEntity, TKey>
		where TEntity : class, IEntityBase<TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IAppInstancesService _appInstances;

		public SampleRestService(IStorageService storage, IExecutionContext execution, IAppInstancesService appInstances)
			: base(storage, execution) { }

		public SampleRestService(IStorageService storage, IExecutionContext execution, IAppInstancesService appInstances, string appTag = "")
			: base(storage, execution, appTag) { }

		protected override TEntity InstanciateEntity()
		{
			return new TEntity();
		}
		protected override IAppInstance GetAppInstanceByTag(string appTag)
		{
			return _appInstances.GetInstanceByTag<TEntity>(appTag);
		}
		protected override IAppInstance GetAppInstanceById(int appInstanceID)
		{
			return _appInstances.GetInstanceById<TEntity>(appInstanceID);
		}
		protected override List<Operation> GetOperations(Query<TEntity> query, HttpVerb verb)
		{
			//On ne permet de filtrer sur certaines opérations qu'en GET
			//Sinon le user pourrait tenter un PUT avec l'opération "view" et ainsi modifier les entités qu'il peut voir !
			//Par contre il a le droit de voir les entités qu'il peut modifier, même si ce n'est pas paramétré comme ça dans ses rôles
			if (query.Options.FilterOperations != null && verb == HttpVerb.GET)
			{
				var filters = Filter.ParseOperations<TEntity>(query.Options.FilterOperations);
				var predicate = new PredicateBuilderHelper(filters).GetPredicate<Operation>();

				var allOperations = _appInstances.GetAllOperations<TEntity>(_appInstance);
				return allOperations.AsQueryable().Where(predicate).ToList();
			}

			return _appInstances.GetOperations<TEntity>(_appInstance, verb); ;
		}

	}
}
