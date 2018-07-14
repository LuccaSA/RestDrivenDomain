using System;
using System.Net;

namespace RDD.Web.Healthz
{
    public class HealthzOptions
    {
        public HealthzOptions()
        {
            HttpStatusCodeFromState = (state) =>
            {
                switch (state)
                {
                    case CheckState.Failed:
                        return HttpStatusCode.InternalServerError;
                    case CheckState.Warn:
                        return HttpStatusCode.PartialContent;
                    case CheckState.Pass:
                        return HttpStatusCode.OK;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }
        /// <summary>
        /// Application or service name
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// Application or service Guid
        /// </summary>
        public Guid ServiceGuid { get; set; }
        /// <summary>
        /// Delegate to select HttpStatusCode for each CheckState result
        /// </summary>
        public Func<CheckState, HttpStatusCode> HttpStatusCodeFromState { get; set; }
    }
}