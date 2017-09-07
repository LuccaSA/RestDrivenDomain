using RDD.Domain.Contracts;
using RDD.Domain.Models.Rights;
using RDD.Infra.Repositories.Mappings;
using RDD.Infra.Services;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra.Repositories.Dtos
{
	public class DtoRepository<T, TInfra> : DtoReadableRepository<T, TInfra>, IRepository<T>
		where T : class
		where TInfra : class
	{
		IRepository<TInfra> _subRepo;
		IMapper<T, TInfra> _mapper;

		public DtoRepository(IRepository<TInfra> subRepo, IStorageService storageService, IReadRightService<T> rightService, IQueryableConvertor<T, TInfra> convertor, IMapper<T, TInfra> mapper, IMapper<TInfra, T> outMapper)
			: base(storageService, rightService, convertor, outMapper)
		{
			_subRepo = subRepo;
			_mapper = mapper;
		}

		public T Add(T input) => _outMapper.Map(_subRepo.Add(_mapper.Map(input)));
		public void AddRange(IEnumerable<T> input) => _subRepo.AddRange(input.Select(_mapper.Map));

		public T Update(T input) => _outMapper.Map(_subRepo.Update(_mapper.Map(input)));

		public void Delete(T input) => _subRepo.Delete(_mapper.Map(input));
		public void DeleteRange(IEnumerable<T> input) => _subRepo.DeleteRange(input.Select(_mapper.Map));
	}
}