using RDD.Infra;
using RDD.Samples.MultiEfContexts.SharedKernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.MultiEfContexts.SharedKernel
{
	public interface IApplicationsService
	{
		void RegisterApplication(Application application);
	}
}
