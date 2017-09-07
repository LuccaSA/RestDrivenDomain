namespace RDD.Infra.Repositories.Mappings
{
	public interface IMapper<TInput, TOutput>
    {
		TOutput Map(TInput input);
    }
}
