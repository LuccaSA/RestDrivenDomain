using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Rdd.Web.Helpers
{
    /// <summary>
    /// Static configuration, for parameters need on OnConfigure step
    /// </summary>
    public class RddBuilder
    {
        public RddBuilder(IServiceCollection services)
        {
            Services = services;
            JsonConverters = new List<JsonConverter>();
        }

        internal IServiceCollection Services { get; }

        internal List<JsonConverter> JsonConverters { get; }
    }
}