using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public class WebPage : Page
    {
        private const int MAX_LIMIT = 1000;
        public static WebPage Default => new WebPage(0, 10);

        public WebPage(int offset, int limit)
            : base(offset, limit, MAX_LIMIT) { }
    }
}
