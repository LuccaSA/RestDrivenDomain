using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IApplication
	{
		string Id { get; }
		string Name { get; }
		ICollection<Operation> Operations { get; }
		ICollection<Combination> Combinations { get; }
	}
}
