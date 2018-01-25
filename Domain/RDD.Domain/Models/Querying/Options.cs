using System;
using System.Collections.Generic;

namespace RDD.Domain.Models.Querying
{
    public class Options
    {
        /// <summary>
        /// Est-ce qu'on a besoin du Count
        /// </summary>
        public bool NeedCount { get; set; }

        /// <summary>
        /// Est-ce qu'on a besoin d'énumérer la query
        /// </summary>
        public bool NeedEnumeration { get; set; }

        /// <summary>
        /// Should we FilterRights on GET request, or CheckRightForCreate on POST
        /// </summary>
        public bool CheckRights { get; set; }

        public bool AttachOperations { get; set; }
        public bool AttachActions { get; set; }
        public bool WithWarnings { get; set; }

        public String Accept { get; set; }

        public Dictionary<string, string> FilterOperations { get; set; }
        public int ImpersonatedPrincipal { get; set; }

        public Options()
        {
            NeedEnumeration = true;
            CheckRights = true;
            WithWarnings = true;
        }
    }
}