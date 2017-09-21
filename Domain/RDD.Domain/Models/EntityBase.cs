using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public abstract class EntityBase<TEntity, TKey> : IEntityBase<TEntity, TKey>, IIncludable
		where TEntity : class
		where TKey : IEquatable<TKey>
	{
		public abstract TKey Id { get; set; }
		public abstract string Name { get; set; }
		public string Url { get; set; }
		public ICollection<Operation> AuthorizedOperations { get; set; }
		public Dictionary<string, bool> AuthorizedActions { get; set; }

		public EntityBase()
		{
			AuthorizedOperations = new HashSet<Operation>();
			AuthorizedActions = new Dictionary<string, bool>();
		}

		public virtual object GetId()
		{
			return Id;
		}
		public virtual void SetId(object id)
		{
			Id = (TKey)id;
		}

		public virtual TEntity Clone()
		{
			return (TEntity)this.MemberwiseClone();
		}
	}
}
