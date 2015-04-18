using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IPermission
	{
		int RoleID { get; set; }
		IRole Role { get; set; }
		int AppInstanceID { get; set; }
		IAppInstance AppInstance { get; set; }
		int OperationID { get; set; }
		Operation Operation { get; set; }
	}
}

