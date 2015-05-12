using RDD.Samples.MultiEfContexts.SharedKernel.Services;
using RDD.Domain.Services;
using RDD.Infra;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.BoundedContextA.Services
{
	public class UsersService : SampleRestService<User, int>, IUsersService
	{
		public UsersService(IStorageService storage, IExecutionContext execution, IAppInstancesService appInstances)
			: base(storage, execution, appInstances) { }
	}
}
