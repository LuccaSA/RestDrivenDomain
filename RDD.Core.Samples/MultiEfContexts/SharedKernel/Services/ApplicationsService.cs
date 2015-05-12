using RDD.Infra;
using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using RDD.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.SharedKernel.Services
{
	public class ApplicationsService : SampleRestService<Application, string>, IApplicationsService
	{
		public ApplicationsService(IStorageService storage, IExecutionContext execution)
			: base(storage, execution, null)
		{
		}

		protected override IAppInstance GetAppInstanceByTag(string appTag) { return null; }
		protected override IAppInstance GetAppInstanceById(int appInstanceID) { return null; }
		public void RegisterApplication(Application application)
		{
			_storage.Add<Application>(application);
		}
	}
}
