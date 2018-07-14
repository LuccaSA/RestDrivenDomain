using System;

namespace RDD.Web.Healthz
{
    public class PingInfos
    {
        public PingInfos()
        {
            TimeUtc = DateTime.UtcNow;
        }

        public string Description { get; set; }
        public string Hostname { get; set; }
        public string AssemblyVersion { get; set; }
        public DateTime TimeUtc { get; }
    }
}