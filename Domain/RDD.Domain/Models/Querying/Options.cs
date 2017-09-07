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

		public bool withPagingInfo { get { return Page != null; } }
		public Page Page { get; set; }
		public bool withMetadata { get; set; }
		public bool attachOperations { get; set; }
		public bool attachActions { get; set; }
		public bool withTemplate { get; set; }
		public bool withWarnings { get; set; }

		public String Accept { get; set; }

		public Dictionary<string, string> FilterOperations { get; set; }
		public int impersonatedPrincipal { get; set; }

		public Options()
		{
			NeedEnumeration = true;
			NeedFilterRights = true;
			withWarnings = true;
		}
	}
}