using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IDownloadableEntity : IEntityBase
	{
		string Src { get; set; }
	}

	public interface IDownloadableEntity<TEntity, TKey> : IDownloadableEntity, IEntityBase<TEntity, TKey>
	{

	}
}
