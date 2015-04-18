using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IRole : IEntityBase<int>
	{
		int Id { get; }
		string Name { get; }
	}
}
