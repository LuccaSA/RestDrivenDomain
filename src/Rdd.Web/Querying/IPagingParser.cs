using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IPagingParser
    {
        Page Parse(string input);
    }
}