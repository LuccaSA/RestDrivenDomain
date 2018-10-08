using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
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
