using RDD.Infra;
using RDD.Infra.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Core.Samples.MultiEfContexts.BoundedContextA.Models
{
	public class AppInstance : EntityBase<IAppInstance, int>, IAppInstance
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
		public string Tag { get; private set; }
		public string ApplicationID { get; private set; }
		public IApplication Application { get; private set; }
	}
}
