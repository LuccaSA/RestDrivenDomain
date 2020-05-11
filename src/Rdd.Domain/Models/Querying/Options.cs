using System.Collections.Generic;

namespace Rdd.Domain.Models.Querying
{
    public class Options
    {
        public bool NeedsCount { get; set; }

        public bool NeedsEnumeration { get; set; }

        public bool ChecksRights { get; set; }

        public bool NeedsDataTracking { get; set; }

        public Dictionary<string,object> CustomOptions { get; set; }

        public Options()
        {
            NeedsEnumeration = true;
            ChecksRights = true;
            NeedsDataTracking = true;
        }
    }
}