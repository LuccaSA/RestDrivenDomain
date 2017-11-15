using System.Net;

namespace RDD.Domain.Exceptions
{
    /// <summary>
    /// HttpStatusCode exposition for domain exceptions
    /// </summary>
    public interface IStatusCodeException
    {
        HttpStatusCode StatusCode { get; }
    }
}