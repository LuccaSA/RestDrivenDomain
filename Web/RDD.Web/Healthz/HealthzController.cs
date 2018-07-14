using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RDD.Web.Healthz
{
    public class HealthzController : Controller
    {
        private readonly HealthzReportService _healthReportService;
        private readonly IOptions<HealthzOptions> _healthzOptions;

        public HealthzController(HealthzReportService healthReportService, IOptions<HealthzOptions> healthzOptions)
        {
            _healthReportService = healthReportService;
            _healthzOptions = healthzOptions;
        }

        [HttpGet]
        [Route("healthz")]
        [Produces("application/json")]
        public async Task<ActionResult<HealthzReport>> Healthz()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            settings.Converters.Add(new StringEnumConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;

            var health = await _healthReportService.GetHealthReport();
            
            var json = Json(health, settings);
            json.StatusCode = (int)_healthzOptions.Value.HttpStatusCodeFromState(health.Status);
            return json;
        }

        [HttpGet]
        [Route("ping")]
        [Produces("application/json")]
        public PingInfos Ping()
        {
            return new PingInfos
            {
                Description = _healthzOptions.Value.ServiceName,
                Hostname = Environment.MachineName,
                AssemblyVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version
            };
        }
    }
}