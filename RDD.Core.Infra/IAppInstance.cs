using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IAppInstance : IEntityBase<int>
	{
		int Id { get; }
		string Name { get; }
		string Tag { get; }
		string ApplicationID { get; }
		IApplication Application { get; }
	}
}
