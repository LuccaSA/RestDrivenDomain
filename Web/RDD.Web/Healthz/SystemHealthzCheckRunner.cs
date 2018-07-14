using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RDD.Web.Healthz
{
    public class SystemHealthzCheckRunner : IHealthzCheckRunner
    {
        private readonly string _componentType = "system";

        public Task<Dictionary<string, List<HealthzCheck>>> GetStatus()
        {
            return Task.FromResult(new Dictionary<string, List<HealthzCheck>>
            {
                { "system uptime", SystemUptime() },
                { "app uptime", ApplicationUptime() },
                { "app RAM", ApplicationRAM() },
            });
        }

        private List<HealthzCheck> SystemUptime()
        {
            return new List<HealthzCheck>
            {
                new HealthzCheck
                {
                    ComponentType = _componentType,
                    Status = CheckState.Pass,
                    MetricValue = TimeSpan.FromMilliseconds(Environment.TickCount).ToString("g"),
                    MetricUnit = "TimeSpan"
                }
            };
        }

        private List<HealthzCheck> ApplicationUptime()
        {
            return new List<HealthzCheck>
            {
                new HealthzCheck
                {
                    ComponentType = _componentType,
                    Status = CheckState.Pass,
                    MetricValue =  (DateTime.UtcNow - UpTime.StartDateTime).ToString("g"),
                    MetricUnit = "TimeSpan"
                }
            };
        }

        private List<HealthzCheck> ApplicationRAM()
        {
            return new List<HealthzCheck>
            {
                new HealthzCheck
                {
                    ComponentType = "memory:utilization",
                    Status = CheckState.Pass,
                    MetricValue =  SizeSuffix(Process.GetCurrentProcess().WorkingSet64,3),
                    MetricUnit = "memory size"
                }
            };
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        // https://stackoverflow.com/a/14488941/533686
        private static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}