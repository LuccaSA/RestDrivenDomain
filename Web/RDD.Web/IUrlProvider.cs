using RDD.Domain;
using System;

namespace RDD.Web
{
    public interface IUrlProvider
    {
        string GetUrlTemplateFromEntityType(Type entityType, IEntityBase entityId);
        string GetApiPrefix();
    }
}
