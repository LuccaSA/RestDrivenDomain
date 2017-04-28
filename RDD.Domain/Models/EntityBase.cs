using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public abstract class EntityBase<TEntity, TKey> : IEntityBase<TEntity, TKey>, IValidatableObject, IIncludable
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

		public void Validate(IStorageService storage, TEntity oldEntity)
		{
			var validationContext = new ValidationContext(this, null, new Dictionary<object, object> { { "storageService", storage } });
			Validator.ValidateObject(this, validationContext, true);
		}
		public void Validate()
		{
			Validate(null, null);
		}
		/// <summary>
		/// http://odetocode.com/blogs/scott/archive/2011/06/29/manual-validation-with-data-annotations.aspx
		/// </summary>
		/// <param name="results"></param>
		/// <returns></returns>
		public bool TryValidate(out ICollection<ValidationResult> results)
		{
			var context = new ValidationContext(this, serviceProvider: null, items: null);
			results = new List<ValidationResult>();
			return Validator.TryValidateObject(this, context, results, validateAllProperties: true);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return new HashSet<ValidationResult>();
		}

		public virtual TEntity Clone()
		{
			return (TEntity)this.MemberwiseClone();
		}
	}
}
