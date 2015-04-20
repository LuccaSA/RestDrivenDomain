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
		IAppInstance GetInstanceByTag<IEntity>(string tag);
		IAppInstance GetInstanceById<IEntity>(int appInstanceID);
		List<Operation> GetOperations<IEntity>(IAppInstance instance, HttpVerb verb);
		List<Operation> GetAllOperations<IEntity>(IAppInstance instance);
	}
}
