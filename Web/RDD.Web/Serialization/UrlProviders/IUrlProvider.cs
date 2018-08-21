using RDD.Domain;
using System;

namespace RDD.Web.Serialization.UrlProviders
{
    public interface IUrlProvider
    {
        string GetApiControllerName(Type workingType);

        Uri GetEntityApiUri(IPrimaryKey entity);
        Uri GetEntityApiUri(Type workingType, IPrimaryKey entity);
    }
}