using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Web.Healthz
{
    public interface IHealthzCheckRunner
    {
        Task<Dictionary<string, List<HealthzCheck>>> GetStatus();
    }
}