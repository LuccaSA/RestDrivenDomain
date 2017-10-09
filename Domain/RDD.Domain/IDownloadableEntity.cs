namespace RDD.Domain
{
	public interface IDownloadableEntity : IEntityBase
	{
		string Src { get; set; }
	}

	public interface IDownloadableEntity<TEntity, TKey> : IDownloadableEntity, IEntityBase<TEntity, TKey>
	{

	}
}
