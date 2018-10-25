using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Rdd.Domain;
using Rdd.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Web.Serialization.UrlProviders
{
    public class UrlProvider : IUrlProvider
    {
        private const string ActionName = nameof(ReadOnlyWebController<IEntityBase<int>, int>.GetByIdAsync);

        private readonly object _lock = new object();

        private IReadOnlyDictionary<Type, string> _templates;
        private string _urlBase;

        private readonly IActionDescriptorCollectionProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlProvider(IActionDescriptorCollectionProvider provider, IHttpContextAccessor httpContextAccessor)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Uri GetEntityApiUri(IPrimaryKey entity)
        {
            if (_templates == null)
            {
                lock (_lock)
                {
                    if (_templates == null)
                    {
                        _templates = CompileUrls();
                    }
                }
            }

            var testedType = GetMatchingType(entity.GetType());
            if (testedType == null)
            {
                return null;
            }

            return new UriBuilder(GetUrlBase()) { Path = GetPath(testedType, entity) }.Uri;
        }

        protected virtual Dictionary<Type, string> CompileUrls()
        {
            var result = new Dictionary<Type, string>();

            var urlProviderActions = _provider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(a => a.ActionName == ActionName);

            foreach (var action in urlProviderActions)
            {
                var entityType = GetEntityType(action.ControllerTypeInfo);
                if (entityType != null)
                {
                    result[entityType] = action.AttributeRouteInfo.Template.Replace("{id}", "{0}");
                }
            }

            return result;
        }

        protected Type GetEntityType(Type controllerType)
        {
            if (controllerType == null)
            {
                return null;
            }

            if (controllerType.IsConstructedGenericType)
            {
                var definition = controllerType.GetGenericTypeDefinition();

                if (typeof(WebController<,>) == definition || typeof(ReadOnlyWebController<,>) == definition)
                {
                    return controllerType.GenericTypeArguments[0];
                }

                if (typeof(WebController<,,,>) == definition || typeof(ReadOnlyWebController<,,>) == definition)
                {
                    return controllerType.GenericTypeArguments[1];
                }
            }

            return GetEntityType(controllerType.BaseType);
        }

        protected virtual Type GetMatchingType(Type initialType)
        {
            if (initialType == null)
            {
                return null;
            }

            if (_templates.ContainsKey(initialType))
            {
                return initialType;
            }

            return GetMatchingType(initialType.BaseType);
        }

        protected virtual string GetUrlBase()
        {
            if (_urlBase == null)
            {
                _urlBase = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}/";
            }

            return _urlBase;
        }

        protected virtual string GetPath(Type type, IPrimaryKey entity)
            => string.Format(_templates[type], entity.GetId());
    }
}