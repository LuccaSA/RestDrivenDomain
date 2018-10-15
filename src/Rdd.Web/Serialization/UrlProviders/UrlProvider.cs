using Microsoft.AspNetCore.Http;
using Rdd.Domain;
using System;

namespace Rdd.Web.Serialization.UrlProviders
{
    public class UrlProvider : IUrlProvider
    {
        protected virtual string ApiPrefix => "api";

        private readonly IPluralizationService _pluralizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlProvider(IPluralizationService pluralizationService, IHttpContextAccessor httpContextAccessor)
        {
            _pluralizationService = pluralizationService ?? throw new ArgumentNullException(nameof(pluralizationService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public virtual Uri GetEntityApiUri(IPrimaryKey entity) => GetEntityApiUri(entity.GetType(), entity);
        public virtual Uri GetEntityApiUri(Type workingType, IPrimaryKey entity)
        {
            if (entity != null)
            {
                var id = entity.GetId();
                var request = _httpContextAccessor.HttpContext.Request;
                return new UriBuilder($"{request.Scheme}://{request.Host.Value}/")
                {
                    Path = string.Format("{0}/{1}{2}", ApiPrefix, GetApiControllerName(workingType).ToLower(), id == null ? null : ("/" + id))
                }.Uri;
            }
            return null;
        }

        public virtual string GetApiControllerName(Type workingType) => _pluralizationService.GetPlural(workingType.Name).ToLower();
    }
}