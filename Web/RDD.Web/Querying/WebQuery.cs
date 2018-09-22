using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public class WebQuery<TEntity> : Query<TEntity>
        where TEntity : class
    {
        public WebQuery()
            : base()
        {
            Page = WebPage.Default;
        }
    }
}
