using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace RDD.Web.Healthz
{
    public class HealthzReportService
    {
        private readonly IEnumerable<IHealthzCheckRunner> _heahlthCheckRunners;
        private readonly IOptions<HealthzOptions> _healthOptions;

        public HealthzReportService(IEnumerable<IHealthzCheckRunner> heahlthCheckRunners, IOptions<HealthzOptions> healthOptions)
        {
            _heahlthCheckRunners = heahlthCheckRunners;
            _healthOptions = healthOptions;
        }

        /// <summary>
        /// Calls all registred HealthCheckRunner, and aggregates results in a HealthzCheck report
        /// All runners are runned in parallel
        /// </summary>
        /// <returns></returns>
        public async Task<HealthzReport> GetHealthReport()
        {
            var options = _healthOptions.Value;
            var healthReport = new HealthzReport(options.ServiceGuid, options.ServiceName);

            var reports = await Task.WhenAll(_heahlthCheckRunners.Select(RunRunner));

            foreach (var report in reports.SelectMany(i => i))
            {
                healthReport.Details.Add(report.Key, report.Value);
            }

            return healthReport;
        }

        private static async Task<Dictionary<string, List<HealthzCheck>>> RunRunner(IHealthzCheckRunner heahlthCheckRunner)
        {
            Dictionary<string, List<HealthzCheck>> status;
            try
            {
                status = await heahlthCheckRunner.GetStatus();
            }
            catch (Exception e)
            {
                status = new Dictionary<string, List<HealthzCheck>>
                {
                    {
                        heahlthCheckRunner.GetType().Name, new List<HealthzCheck>
                        {
                            new HealthzCheck
                            {
                                ComponentType = "HeahlthCheck:fatalerror",
                                Status = CheckState.Failed,
                                Output = e.ToString()
                            }
                        }
                    }
                };
            }
            return status;
        }
    }
}