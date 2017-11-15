using Microsoft.AspNetCore.Http;
using RDD.Domain;
using System;

namespace RDD.Web.Serialization
{
    public class UrlProvider : IUrlProvider
    {
        protected virtual string ApiPrefix => "api";

        private readonly PluralizationService _pluralizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlProvider(IHttpContextAccessor httpContextAccessor)
        {
            _pluralizationService = new PluralizationService();
            _httpContextAccessor = httpContextAccessor;
        }

        protected virtual Type GetEntityType(IEntityBase entity)
        {
            return entity.GetType();
        }

        public virtual string GetEntityUrl(IEntityBase entity)
        {
            var entityType = GetEntityType(entity);

            var entityName = _pluralizationService.GetPlural(entityType.Name).ToLower();
            var request = _httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.Value}/{ApiPrefix}/{entityName}/{entity.GetId()}";
        }
    }
}
