using Microsoft.AspNetCore.Http;
using RDD.Domain;

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

        public virtual string GetEntityUrl(IEntityBase entity)
        {
            var entityType = entity.GetType();

            var entityName = _pluralizationService.GetPlural(entityType.Name).ToLower();
            var request = _httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.Value}/{ApiPrefix}/{entityName}/{entity.GetId()}";
        }
    }
}
