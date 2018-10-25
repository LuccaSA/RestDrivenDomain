using Microsoft.AspNetCore.Http;
using Rdd.Infra.Web.Models;

namespace Rdd.Web.Querying
{
    public interface IPagingParser
    {
        Page Parse(HttpRequest request);
    }
}