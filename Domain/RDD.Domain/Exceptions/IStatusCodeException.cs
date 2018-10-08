using System.Net;

namespace Rdd.Domain.Exceptions
{
    /// <summary>
    /// HttpStatusCode exposition for domain exceptions
    /// </summary>
    public interface IStatusCodeException
    {
        HttpStatusCode StatusCode { get; }
    }
}