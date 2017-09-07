using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// Whether we need to filter entities based on what curPrincipal can see.
		/// <para>ie: whether we execute FilterRights</para>
		/// </summary>
		public bool NeedFilterRights { get; set; }

		public bool AttachOperations { get; set; }
		public bool AttachActions { get; set; }
		public bool WithWarnings { get; set; }
		public bool WithPagingInfo { get; set; }

		public String Accept { get; set; }

		public Dictionary<string, string> FilterOperations { get; set; }
		public int ImpersonatedPrincipal { get; set; }

		public Options()
		{
			NeedEnumeration = true;
			NeedFilterRights = true;
			WithWarnings = true;
		}
	}
}