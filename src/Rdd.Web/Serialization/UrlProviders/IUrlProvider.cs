using Rdd.Domain;
using System;

namespace Rdd.Web.Serialization.UrlProviders
{
    public interface IUrlProvider
    {
        Uri GetEntityApiUri(IPrimaryKey entity);
    }
}