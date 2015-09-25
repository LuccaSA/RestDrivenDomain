using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IEntityBase
	{
		string Name { get; }
		void Forge(IStorageService storage, IAppInstance appInstance);
		void Validate(IStorageService storage);
		ICollection<Operation> Operations { get; set; }
	}
	public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey>
			where TKey : IEquatable<TKey> { }

}
