using System;

namespace RDD.Web
{
    public interface IUrlProvider
    {
        string GetUrlTemplateFromEntityType(Type entityType);
        string GetApiPrefix();
    }
}
