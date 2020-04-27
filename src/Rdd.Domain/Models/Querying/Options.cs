namespace Rdd.Domain.Models.Querying
{
    public class Options
    {
        public bool NeedsCount { get; set; }

        public bool NeedsEnumeration { get; set; }

        public bool ChecksRights { get; set; }

        public bool NeedsDataTracking { get; set; }

        /// <summary>
        /// Warning : use only in case of multiple includes, and by testing the behavior before and after enabling this.
        /// This property can lead to under-perform in some cases, so use it with caution
        /// https://entityframework-plus.net/query-include-optimized
        /// </summary>
        public bool OptimizeIncludes { get; set; }

        public Options()
        {
            NeedsEnumeration = true;
            ChecksRights = true;
            NeedsDataTracking = true;
        }
    }
}