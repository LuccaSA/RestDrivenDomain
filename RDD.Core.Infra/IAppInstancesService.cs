using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IAppInstancesService
	{
		IAppInstance GetInstanceByTag<TEntity>(string tag);
		IAppInstance GetInstanceById<TEntity>(int appInstanceID);
		List<Operation> GetOperations<TEntity>(IAppInstance instance, HttpVerb verb);
		List<Operation> GetAllOperations<TEntity>(IAppInstance instance);
	}
}
