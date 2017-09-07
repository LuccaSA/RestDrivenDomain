using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RDD.Domain.Models
{
	public abstract class EntityBase<TKey> : IEntityBase<TKey>, IValidatableObject, IIncludable
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

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return new HashSet<ValidationResult>();
		}
	}
}