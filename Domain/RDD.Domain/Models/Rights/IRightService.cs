using RDD.Domain.Models.StorageQueries.Filters;

namespace RDD.Domain.Models.Rights
{
	public interface IReadRightService<T> : IFilter<T> { }

	public interface IRightService<T> : IReadRightService<T>
	{
		bool HasCreationRight(T input);
		void ThrowIfNoCreationRight(T input);

		bool HasEditionRight(T input);
		void ThrowIfNoEditionRight(T input);

		bool HasDeletionRight(T input);
		void ThrowIfNoDeletionRight(T input);
	}
}