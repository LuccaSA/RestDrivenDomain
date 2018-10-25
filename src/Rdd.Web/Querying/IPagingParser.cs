using Microsoft.AspNetCore.Http;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IPagingParser
    {
        Page Parse(HttpRequest request);
    }
}