using Microsoft.AspNetCore.Http;
using System;

namespace RDD.Web.Serialization
{
    public class UrlProvider : IUrlProvider
    {
        private const string DEFAULT_API_PREFIX = "api";

        private readonly PluralizationService _pluralizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlProvider(IHttpContextAccessor httpContextAccessor)
        {
            _pluralizationService = new PluralizationService();
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual string GetApiPrefix()
        {
            return DEFAULT_API_PREFIX;
        }

        public virtual string GetUrlTemplateFromEntityType(Type entityType)
        {
            var apiRadical = _pluralizationService.GetPlural(entityType.Name).ToLower();
            var request = _httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.Value}/{GetApiPrefix()}/{apiRadical}/{{0}}";
        }
    }
}
