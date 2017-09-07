namespace RDD.Domain.Models.Validations
{
	public interface IValidationService<T>
	{
		bool IsValid(T input);
		void ThrowIfInvalid(T input);
	}
}