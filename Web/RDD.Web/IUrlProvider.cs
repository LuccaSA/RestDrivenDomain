using RDD.Domain;
using System;

namespace RDD.Web
{
    public interface IUrlProvider
    {
        string GetEntityUrl(IEntityBase entity);
    }
}
