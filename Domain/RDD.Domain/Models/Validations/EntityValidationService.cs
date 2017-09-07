using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RDD.Domain.Models.Validations
{
	class EntityValidationService<T> : IValidationService<T>
	{
		public bool IsValid(T input)
		{
			return TryValidate(input, out var results);
		}

		public bool TryValidate(T input, out ICollection<ValidationResult> results)
		{
			results = new List<ValidationResult>();
			return Validator.TryValidateObject(input, new ValidationContext(input), results, true);
		}

		public void ThrowIfInvalid(T input)
		{
			Validator.ValidateObject(input, new ValidationContext(input), true);
		}
	}
}
