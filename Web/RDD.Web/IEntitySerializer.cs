using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web
{
	public interface IEntitySerializer
	{
		string GetUrlTemplateFromEntityType(Type entityType);
		Dictionary<string, object> SerializeCollection<TEntity>(IRestCollection<TEntity> collection, Field<TEntity> fields)
			where TEntity : class, IEntityBase;
		List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, Field<TEntity> fields);
		Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, Field<TEntity> fields);

		List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, PropertySelector fields);
		Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector fields);
	}
}
