using RDD.Domain.Contracts;
using RDD.Domain.Models.Rights;
using RDD.Domain.Models.Validations;
using RDD.Infra.Services;
using System.Collections.Generic;

namespace RDD.Infra.Repositories.Simples
{
	public class SimpleRepository<T> : SimpleReadableRepository<T>, IRepository<T> where T : class
	{
		IRightService<T> _rightService;
		IValidationService<T> _validationService;

		public SimpleRepository(IStorageService storageService, IRightService<T> rightService, IValidationService<T> valiationService)
			: base(storageService, rightService)
		{
			_rightService = rightService;
			_validationService = valiationService;
		}

		public T Add(T input)
		{
			_rightService.ThrowIfNoCreationRight(input);
			_validationService.ThrowIfInvalid(input);

			return _storageService.Add(input);
		}

		public void AddRange(IEnumerable<T> input)
		{
			foreach (var entity in input)
			{
				_rightService.ThrowIfNoCreationRight(entity);
				_validationService.ThrowIfInvalid(entity);
			}

			_storageService.AddRange(input);
		}

		public T Update(T input)
		{
			_validationService.ThrowIfInvalid(input);
			return input;
		}

		public void Delete(T input)
		{
			_rightService.ThrowIfNoDeletionRight(input);
			_storageService.Remove(input);
		}

		public void DeleteRange(IEnumerable<T> input)
		{
			foreach (var entity in input)
			{
				_rightService.ThrowIfNoDeletionRight(entity);
			}

			_storageService.RemoveRange(input);
		}
	}
}