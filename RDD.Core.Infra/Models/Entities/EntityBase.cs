using RDD.Infra.Models.Rights;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Entities
{
	public abstract class EntityBase<TEntity, TKey> : IEntityBase<TKey>, IValidatableObject, IIncludable
		where TEntity : class
		where TKey : IEquatable<TKey>
	{
		public abstract TKey Id { get; set; }
		public abstract string Name { get; set; }
		public ICollection<Operation> Operations { get; set; }
		public Dictionary<string, bool> Actions { get; set; }

		public EntityBase()
		{
			Operations = new HashSet<Operation>();
			Actions = new Dictionary<string, bool>();
		}

		public virtual object GetId()
		{
			return Id;
		}
		public virtual void Forge(IStorageService storage, IAppInstance appInstance) { }

		public void Validate(IStorageService storage)
		{
			var validationContext = new ValidationContext(this, null, new Dictionary<object, object> { { "storageService", storage } });
			Validator.ValidateObject(this, validationContext, true);
		}
		public void Validate()
		{
			Validate((IStorageService)null);
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
	}
}
