using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Rights
{
	public class Permission : IPermission
	{
		public int RoleID { get; set; }
		public IRole Role { get; set; }
		public int AppInstanceID { get; set; }
		public IAppInstance AppInstance { get; set; }
		public int OperationID { get; set; }
		public Operation Operation { get; set; }
	}
}
