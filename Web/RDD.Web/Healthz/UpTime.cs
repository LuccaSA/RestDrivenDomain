using System;

namespace RDD.Web.Healthz
{
    internal static class UpTime
    {
        /// <summary>
        /// Used to store application startup DateTime
        /// </summary>
        public static DateTime StartDateTime = DateTime.UtcNow;
    }
}