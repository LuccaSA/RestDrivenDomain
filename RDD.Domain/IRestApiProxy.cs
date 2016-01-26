using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRestApiProxy
	{
		TEntity Get<TEntity, TKey>(string uri) where TEntity : IEntityBase<TEntity, TKey>;
		TEntity Post<TEntity, TKey>(string uri, NameValueCollection data) where TEntity : IEntityBase<TEntity, TKey>;
		IDownloadableEntity<TEntity, TKey> PostFile<TEntity, TKey>(string uri, string filePath);
		TEntity PostJson<TEntity, TKey>(string uri, TEntity entity) where TEntity : IEntityBase<TEntity, TKey>;
		TEntity PutJson<TEntity, TKey>(string uri, TEntity entity) where TEntity : IEntityBase<TEntity, TKey>;
	}
}
