using System;

namespace RDD.Web.Healthz
{
    /// <summary>
    /// Health check result placeholder 
    /// </summary>
    public class HealthzCheck
    {
        public HealthzCheck()
        {
            Time = DateTime.UtcNow;
        }
        public Guid? ComponentId { get; set; }
        public string ComponentType { get; set; }
        public string MetricValue { get; set; }
        public string MetricUnit { get; set; }
        public CheckState Status { get; set; }
        public DateTime Time { get; }
        public string Output { get; set; }
    }
}