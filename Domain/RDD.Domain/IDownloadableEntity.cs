namespace RDD.Domain
{
	public interface IDownloadableEntity : IEntityBase
	{
		string Src { get; set; }
	}

	public interface IDownloadableEntity<TKey> : IDownloadableEntity, IEntityBase<TKey>
	{
	}
}
