using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RDD.Web.Healthz
{
    public class HealthzReport
    {
        public HealthzReport(Guid serviceGuid, string serviceName)
        {
            ServiceName = serviceName;
            ServiceId = serviceGuid.ToString("D");
            Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Details = new Dictionary<string, List<HealthzCheck>>();
        }

        public CheckState Status
        {
            get
            {
                if (Details.Any(i => i.Value.Any(k => k.Status == CheckState.Failed)))
                    return CheckState.Failed;
                if (Details.Any(i => i.Value.Any(k => k.Status == CheckState.Warn)))
                    return CheckState.Warn;
                return CheckState.Pass;
            }
        }

        public string Version { get; }
        public string ServiceId { get; }
        public string ServiceName { get; }
        public Dictionary<string, List<HealthzCheck>> Details { get; }
    }
}